"""Build APK Installer app/tray/exe icons from neon-on-white master art.

Same pipeline as Studio / Beam / Halo (`build-icon-from-art.py`):
unmultiply the cyan neon mark off white, composite onto the family dark
rounded tile, export PNG sizes + a multi-res .ico.

Usage:
    python tools/build-icon-from-art.py
"""

from __future__ import annotations

from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "assets" / "installer-icon-art.png"
SRC_SMALL = ROOT / "assets" / "installer-icon-art-small.png"
APP_ASSETS = ROOT / "src" / "Installer.App" / "Assets"
REPO_ASSETS = ROOT / "assets" / "icons"
BRAND = ROOT / "brand"

MASTER = 1024
PNG_SIZES = [16, 32, 48, 64, 128, 256]
ICO_SIZES = [16, 24, 32, 48, 64, 128, 256]
SMALL_MAX = 24
TINY_MAX = 16

TILE_TOP = np.array([20, 28, 34])
TILE_BOT = np.array([9, 13, 17])
CORNER_R = 0.225


def extract_mark(src: Path) -> Image.Image:
    rgb = np.asarray(Image.open(src).convert("RGBA"), dtype=np.float32)[..., :3]
    alpha = np.clip((1.0 - rgb.min(axis=2) / 255.0 - 0.03) / 0.97, 0, 1)
    af = np.clip(alpha, 1e-4, 1)[..., None]
    fg = np.clip((rgb - (1 - af) * 255.0) / af, 0, 255)
    mark = Image.fromarray(np.dstack([fg, alpha * 255]).astype(np.uint8), "RGBA")
    a = np.asarray(mark)[..., 3]
    ys, xs = np.where(a > 6)
    return mark.crop((int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1))


def core_bbox(mark: Image.Image):
    a = np.asarray(mark)[..., 3]
    ys, xs = np.where(a > 60)
    return int(xs.min()), int(ys.min()), int(xs.max()), int(ys.max())


def tile(size: int) -> Image.Image:
    grad = TILE_TOP + (TILE_BOT - TILE_TOP) * (np.arange(size) / (size - 1))[:, None]
    base = Image.fromarray(np.repeat(grad[:, None, :], size, axis=1).astype(np.uint8), "RGB").convert("RGBA")
    mask = Image.new("L", (size, size), 0)
    ImageDraw.Draw(mask).rounded_rectangle([0, 0, size - 1, size - 1], radius=int(CORNER_R * size), fill=255)
    hi = Image.new("L", (size, size), 0)
    ImageDraw.Draw(hi).rounded_rectangle([0, 0, size - 1, int(size * 0.5)], radius=int(CORNER_R * size), fill=26)
    hi = Image.composite(hi.filter(ImageFilter.GaussianBlur(size * 0.06)), Image.new("L", (size, size), 0), mask)
    base = Image.alpha_composite(base, Image.merge("RGBA", [Image.new("L", (size, size), 255)] * 3 + [hi]))
    base.putalpha(mask)
    return base


def compose(mark: Image.Image, fill: float) -> Image.Image:
    cx0, cy0, cx1, cy1 = core_bbox(mark)
    scale = fill * MASTER / max(cx1 - cx0, cy1 - cy0)
    ms = mark.resize((round(mark.width * scale), round(mark.height * scale)), Image.LANCZOS)
    ox = round(MASTER / 2 - (cx0 + cx1) / 2 * scale)
    oy = round(MASTER / 2 - (cy0 + cy1) / 2 * scale)
    out = tile(MASTER)
    out.alpha_composite(ms, (ox, oy))
    clip = np.minimum(np.asarray(out)[..., 3], np.asarray(tile(MASTER))[..., 3])
    out.putalpha(Image.fromarray(clip.astype(np.uint8)))
    return out


def frame(full: Image.Image, small: Image.Image, tiny: Image.Image, size: int) -> Image.Image:
    src = tiny if size <= TINY_MAX else small if size <= SMALL_MAX else full
    f = src.resize((size, size), Image.LANCZOS)
    if size <= 48:
        f = f.filter(ImageFilter.UnsharpMask(radius=1.0, percent=70, threshold=0))
    return f


def export(full: Image.Image, small: Image.Image, tiny: Image.Image, out_dir: Path) -> None:
    out_dir.mkdir(parents=True, exist_ok=True)
    icon_256 = full.resize((256, 256), Image.LANCZOS)
    icon_256.save(out_dir / "app-icon-256.png")
    icon_256.save(out_dir / "app-icon.png")
    for size in PNG_SIZES:
        frame(full, small, tiny, size).save(out_dir / f"app-icon-{size}.png")
    frames = [frame(full, small, tiny, s) for s in ICO_SIZES]
    frames[-1].save(
        out_dir / "app-icon.ico",
        format="ICO",
        sizes=[(s, s) for s in ICO_SIZES],
        append_images=frames[:-1],
    )


def main() -> None:
    mark = extract_mark(SRC)
    small_src = extract_mark(SRC_SMALL) if SRC_SMALL.exists() else mark
    full = compose(mark, fill=0.78)
    small = compose(small_src, fill=0.86)
    tiny = compose(small_src, fill=0.70)
    for target in (APP_ASSETS, REPO_ASSETS):
        export(full, small, tiny, target)
        print(f"Exported icons to {target}")
    BRAND.mkdir(parents=True, exist_ok=True)
    full.resize((512, 512), Image.LANCZOS).save(BRAND / "icon-512.png")
    full.save(REPO_ASSETS / "source-icon.png")
    print(f"Wrote {BRAND / 'icon-512.png'}")


if __name__ == "__main__":
    main()
