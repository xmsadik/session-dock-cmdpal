"""Regenerate the package icons in src/ClaudeSessions/Assets.

Usage: python scripts/make-icons.py   (needs Pillow)

The mark is a slate rounded square with a white terminal prompt (">_") and a green status dot.
Everything is drawn at 1024 px and downsampled, so small sizes stay smooth.
"""

from pathlib import Path

from PIL import Image, ImageDraw

ASSETS = Path(__file__).resolve().parent.parent / "src" / "ClaudeSessions" / "Assets"
BASE = 1024
TILE = (44, 62, 80, 255)  # slate
INK = (255, 255, 255, 255)
DOT = (34, 197, 94, 255)  # green: a live session


def stroke(d: ImageDraw.ImageDraw, points: list[tuple[float, float]], width: float) -> None:
    """Polyline with round caps and joins."""
    d.line(points, fill=INK, width=int(width), joint="curve")
    r = width / 2
    for x, y in points:
        d.ellipse((x - r, y - r, x + r, y + r), fill=INK)


def icon(size: int) -> Image.Image:
    img = Image.new("RGBA", (BASE, BASE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((0, 0, BASE - 1, BASE - 1), radius=int(BASE * 0.22), fill=TILE)

    # Prompt chevron and cursor, up-left of centre to leave room for the dot.
    w = BASE * 0.095
    stroke(d, [(BASE * 0.22, BASE * 0.27), (BASE * 0.42, BASE * 0.45), (BASE * 0.22, BASE * 0.63)], w)
    stroke(d, [(BASE * 0.47, BASE * 0.63), (BASE * 0.56, BASE * 0.63)], w)

    # Status dot with a tile-coloured ring so it reads as separate from the prompt.
    dx, dy, dr, ring = BASE * 0.76, BASE * 0.76, BASE * 0.13, BASE * 0.045
    d.ellipse((dx - dr - ring, dy - dr - ring, dx + dr + ring, dy + dr + ring), fill=TILE)
    d.ellipse((dx - dr, dy - dr, dx + dr, dy + dr), fill=DOT)

    return img.resize((size, size), Image.LANCZOS)


def centered(width: int, height: int, mark: int) -> Image.Image:
    img = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    img.alpha_composite(icon(mark), ((width - mark) // 2, (height - mark) // 2))
    return img


def main() -> None:
    squares = {
        "StoreLogo.png": 50,
        "LockScreenLogo.scale-200.png": 48,
        "Square44x44Logo.scale-200.png": 88,
        "Square44x44Logo.targetsize-24_altform-unplated.png": 24,
        "Square150x150Logo.scale-200.png": 300,
    }
    for name, size in squares.items():
        icon(size).save(ASSETS / name)

    centered(620, 300, 200).save(ASSETS / "Wide310x150Logo.scale-200.png")
    centered(1240, 600, 360).save(ASSETS / "SplashScreen.scale-200.png")
    print(f"Wrote {len(squares) + 2} icons to {ASSETS}")


if __name__ == "__main__":
    main()
