// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Microsoft.CommandPalette.Extensions;

namespace ClaudeSessions;

[Guid("55E72E20-244F-4979-AF7E-A74B64B95892")]
public sealed partial class ClaudeSessionsExtension : IExtension, IDisposable
{
    private readonly Action<ClaudeSessionsExtension> _onDisposed;

    private readonly ClaudeSessionsCommandsProvider _provider = new();

    private bool _retired;

    /// <param name="onDisposed">Called when the host disposes this instance (decides whether the process exits).</param>
    public ClaudeSessionsExtension(Action<ClaudeSessionsExtension> onDisposed)
    {
        _onDisposed = onDisposed;
    }

    public object? GetProvider(ProviderType providerType)
    {
        return providerType switch
        {
            ProviderType.Commands => _provider,
            _ => null,
        };
    }

    /// <summary>
    /// Stops this instance's polling and notifications without ending the process. Used when a newer
    /// host activation replaces it, so the old host's objects stop toasting and scanning.
    /// </summary>
    public void Retire()
    {
        if (_retired)
        {
            return;
        }

        _retired = true;
        _provider.Dispose();
    }

    public void Dispose()
    {
        Retire();
        _onDisposed(this);
    }
}
