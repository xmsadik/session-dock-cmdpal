using System;
using ClaudeSessions.Core;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace ClaudeSessions;

/// <summary>The single compact Dock item: one fixed vector icon plus plain-text counts; opens <paramref name="target"/> (list or summary page).</summary>
internal sealed partial class SessionsBand : ListItem, IDisposable
{
    private readonly SessionStore _store;

    public SessionsBand(SessionStore store, ICommand target)
        : base(target)
    {
        _store = store;
        Icon = Glyphs.Band; // a single fixed vector icon; state lives in the plain-text title
        _store.Changed += OnStoreChanged;
        Render();
    }

    public void Dispose() => _store.Changed -= OnStoreChanged;

    private void OnStoreChanged(object? sender, EventArgs e)
    {
        try
        {
            Render();
        }
        catch (Exception ex)
        {
            DiagLog.WriteLine($"SessionsBand.Render failed: {ex}");
        }
    }

    private void Render()
    {
        var sessions = _store.Sessions;
        Title = SessionSummary.CountsText(sessions);
        Subtitle = SessionSummary.Subtitle(sessions);
    }
}
