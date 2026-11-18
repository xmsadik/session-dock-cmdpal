using Microsoft.CommandPalette.Extensions.Toolkit;

namespace ClaudeSessions;

/// <summary>
/// Segoe Fluent Icons glyphs. Monochrome vectors, so they follow the Command Palette theme like the
/// built-in Dock icons. Code points are written as numbers to keep the source ASCII-only.
/// </summary>
internal static class Glyphs
{
    /// <summary>The one Dock band icon (CommandPrompt); session state shows in the title text instead.</summary>
    public static readonly IconInfo Band = From(0xE756);

    private static IconInfo From(int codePoint) => new(char.ConvertFromUtf32(codePoint));
}
