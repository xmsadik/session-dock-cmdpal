// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;
using System;
using ClaudeSessions.Core;
using System.Threading;
using System.Threading.Tasks;

namespace ClaudeSessions;

public class Program
{
    [MTAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "-RegisterProcessAsComServer")
        {
            global::Shmuelie.WinRTServer.ComServer server = new();

            ManualResetEvent extensionDisposedEvent = new(false);

            // One extension instance per host activation, not one per process. This process outlives a
            // Command Palette restart (crash, PowerToys restart, another extension's redeploy) and the new
            // host activates it again. Reusing the old objects is broken: they still hold the dead host's
            // PropChanged/ItemsChanged subscriptions, and the toolkit raises those events inside a
            // try/catch, so the first (dead) subscriber throws and the live host never hears a change.
            // The Dock then freezes on the count it had when the old host died.
            // Command Palette is single-instance, so a new activation means the previous host is gone:
            // retire the previous instance (stop its polling and toasts) and hand out fresh objects.
            object gate = new();
            ClaudeSessionsExtension? current = null;
            server.RegisterClass<ClaudeSessionsExtension, IExtension>(() =>
            {
                lock (gate)
                {
                    var previous = current;
                    current = new ClaudeSessionsExtension(disposed =>
                    {
                        lock (gate)
                        {
                            // Exit only when the live host disposes its instance; a retired one is ignored.
                            if (ReferenceEquals(disposed, current))
                            {
                                extensionDisposedEvent.Set();
                            }
                        }
                    });

                    if (previous is not null)
                    {
                        DiagLog.WriteLine("New host activation: retiring the previous extension instance.");
                        try
                        {
                            previous.Retire();
                        }
                        catch (Exception ex)
                        {
                            DiagLog.WriteLine($"Retiring the previous extension instance failed: {ex}");
                        }
                    }

                    return current;
                }
            });
            server.Start();

            // Wait until the live host disposes its extension instance.
            extensionDisposedEvent.WaitOne();
            server.Stop();
            server.UnsafeDispose();
        }
    }
}
