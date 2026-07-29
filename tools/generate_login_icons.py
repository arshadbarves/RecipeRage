"""Generates login button icons (Google G, Facebook f, guest person) as 256x256 PNGs."""
import math
import os
from PIL import Image, ImageDraw

SIZE = 256
OUT_DIR = "Assets/Resources/UI/Icons"


def google():
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cx, cy, r = SIZE / 2, SIZE / 2, 108
    w = 52  # stroke width
    bbox = [cx - r, cy - r, cx + r, cy + r]
    # Four arcs (degrees, PIL measures clockwise from 3 o'clock going down)
    d.arc(bbox, start=305, end=45, fill=(66, 133, 244, 255), width=w)    # blue (right/top-right)
    d.arc(bbox, start=45, end=155, fill=(234, 67, 53, 255), width=w)     # red (top)
    d.arc(bbox, start=155, end=235, fill=(251, 188, 5, 255), width=w)    # yellow (left/bottom-left)
    d.arc(bbox, start=235, end=305, fill=(52, 168, 83, 255), width=w)    # green (bottom)
    # Blue horizontal bar of the G
    bar_h = w
    d.rectangle([cx, cy - bar_h / 2, cx + r, cy + bar_h / 2], fill=(66, 133, 244, 255))
    return img


def facebook():
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    white = (255, 255, 255, 255)
    d = ImageDraw.Draw(img)
    x0 = 104
    stem_w = 56
    # stem
    d.rectangle([x0, 60, x0 + stem_w, 236], fill=white)
    # hook: outer circle top-right
    d.ellipse([x0, 28, x0 + 132, 160], fill=white)
    # crossbar
    d.rectangle([x0 - 36, 116, x0 + 108, 160], fill=white)
    # erase hook interior (inner circle), keeping the stem intact
    inner = Image.new("L", (SIZE, SIZE), 0)
    di = ImageDraw.Draw(inner)
    di.ellipse([x0 + 52, 80, x0 + 140, 168], fill=255)
    px = img.load()
    for y in range(SIZE):
        for x in range(SIZE):
            if inner.getpixel((x, y)) > 0 and not (x0 <= x <= x0 + stem_w):
                px[x, y] = (255, 255, 255, 0)
    return img


def guest():
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    white = (255, 255, 255, 255)
    stroke = 22
    cx = SIZE / 2
    # Head circle (outline)
    hr = 42
    hy = 66
    d.ellipse([cx - hr, hy - hr, cx + hr, hy + hr], outline=white, width=stroke)
    # Shoulders arc (outline)
    sr = 74
    sy = 196
    d.arc([cx - sr, sy - sr, cx + sr, sy + sr], start=180, end=360, fill=white, width=stroke)
    return img


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    google().save(os.path.join(OUT_DIR, "icon_google.png"))
    facebook().save(os.path.join(OUT_DIR, "icon_facebook.png"))
    guest().save(os.path.join(OUT_DIR, "icon_guest.png"))
    print("Generated icon_google.png, icon_facebook.png, icon_guest.png in", OUT_DIR)


if __name__ == "__main__":
    main()
