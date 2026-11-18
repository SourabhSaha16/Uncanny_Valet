"""Cuts the sliced Valhalla lot sprites into a SQUARE tile set for Tilemap painting (hand-made lots, #194).

Run after tools/slice-valhalla.py: `python tools/slice-valhalla-tiles.py` from the repository root (needs Pillow and numpy). It reads
Assets/_Project/Art/ValhallaMidgard/Lot/*.png and writes 88x88 px tiles (4x4 units at 22 pixels per unit) to
Assets/_Project/Art/ValhallaMidgard/Tiles/, replacing what was there. The sandbox sprites in Lot/ are never touched.

The AI tile cards carry a dark outline and a bevel, which would show as a grid when two tiles meet. Flat ground tiles are cropped by
INSET pixels on every side, then resampled to 88x88 with nearest neighbour (hard pixel edge, like the source). Tall or wide pieces are
resampled to the square (walls) or fitted and padded (pilasters, awnings), so every tile is exactly TILE x TILE.

`--preview PATH` also writes a contact sheet of every tile and a 3x3 seam test of each flat tile to PATH.
"""
import os
import shutil
import sys

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
LOT = os.path.join(ROOT, "Assets", "_Project", "Art", "ValhallaMidgard", "Lot")
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "ValhallaMidgard", "Tiles")

TILE = 88          # pixels; 4 units at 22 pixels per unit
FLAT_INSET = 5     # outline and bevel cropped from flat ground tiles
HEDGE_INSET = 3    # the hedge keeps its leafy edge
WALL_INSET = 1     # walls keep their trim

# Output group prefix, output name, source sprite, mode. Modes: flat (inset crop, opaque), hedge, wall (resample), pad_l/pad_r (fit height, align side),
# awning (fit width, align top).
FLAT = [
    ("ground", "asphalt", "asphalt_plain"), ("ground", "asphalt_speckle", "asphalt_speckle"), ("ground", "asphalt_cracked", "asphalt_cracked"),
    ("ground", "asphalt_skid", "asphalt_skid"), ("ground", "grass", "grass"),
    ("marking", "edge_w", "edge_line_w"), ("marking", "edge_e", "edge_line_e"), ("marking", "edge_n", "edge_line_n"),
    ("marking", "edge_s", "edge_line_s"), ("marking", "bay_empty", "bay_empty"), ("marking", "bay_p", "bay_p"),
    ("marking", "bay_handicap", "bay_handicap"), ("marking", "crosswalk_h", "crosswalk_h"), ("marking", "crosswalk_v", "crosswalk_v"),
    ("marking", "crosswalk_v_thin", "crosswalk_v_thin"), ("marking", "crosswalk_stagger", "crosswalk_stagger"),
    ("marking", "centre_h", "centre_line_h"), ("marking", "centre_v", "centre_line_v"),
    ("sidewalk", "plain", "sidewalk_plain"), ("sidewalk", "corner_a", "sidewalk_corner_a"), ("sidewalk", "corner_b", "sidewalk_corner_b"),
    ("sidewalk", "t", "sidewalk_t"), ("sidewalk", "cross", "sidewalk_cross"),
    ("sidewalk", "curb_straight", "road_curb_straight"), ("sidewalk", "curb_corner_outer", "road_curb_corner_outer"),
    ("sidewalk", "curb_corner_inner", "road_curb_corner_inner"), ("sidewalk", "curb_t", "road_curb_t"), ("sidewalk", "curb_cross", "road_curb_cross"),
]
HEDGE = [("hedge", "block", "hedge_block"), ("hedge", "corner_a", "hedge_corner_a"), ("hedge", "corner_b", "hedge_corner_b"),
         ("hedge", "t", "hedge_t"), ("hedge", "cross", "hedge_cross")]
WALL = [("building", "wall", "wall_plain"), ("building", "window_1", "wall_window_1"), ("building", "window_2", "wall_window_2"),
        ("building", "window_3", "wall_window_3"), ("building", "window_4", "wall_window_4"), ("building", "door_single", "wall_door_single"),
        ("building", "door_double", "wall_door_double"), ("building", "corner", "wall_corner")]
PADDED = [("building", "pilaster_left", "wall_pilaster_left", "left"), ("building", "pilaster_right", "wall_pilaster_right", "right")]
AWNINGS = [("overhead", "awning_left", "awning_left", "left"), ("overhead", "awning_mid_a", "awning_mid_a", "centre"),
           ("overhead", "awning_mid_b", "awning_mid_b", "centre"), ("overhead", "awning_right", "awning_right", "right")]
# The roof floor is cropped from the middle of the sprite sheet roof (81 px tall, so the middle 81 px square), edges get a frame bar.
ROOF_FLOOR_CROP = (24, 0, 105, 81)
ROOF_BARS = {"n": ("roof_frame_top", "top"), "s": ("roof_frame_bottom", "bottom"), "w": ("roof_frame_left", "left"), "e": ("roof_frame_left", "right")}


def load(name):
    return Image.open(os.path.join(LOT, name + ".png")).convert("RGBA")


def inset_square(name, inset):
    image = load(name)
    w, h = image.size
    return image.crop((inset, inset, w - inset, h - inset)).resize((TILE, TILE), Image.NEAREST)


def fitted(name, align):
    """Scaled to the tile height (no upscaling past it), then placed on a transparent tile."""
    image = load(name)
    scale = min(TILE / image.height, TILE / image.width)
    size = (max(1, round(image.width * scale)), max(1, round(image.height * scale)))
    image = image.resize(size, Image.NEAREST)
    tile = Image.new("RGBA", (TILE, TILE), (0, 0, 0, 0))
    x = {"left": 0, "right": TILE - size[0], "centre": (TILE - size[0]) // 2}[align]
    tile.alpha_composite(image, (x, 0))
    return tile


def awning(name, align):
    """Awnings hang from the top of the tile; kept at source size, only shrunk when wider than the tile."""
    image = load(name)
    if image.width > TILE:
        image = image.resize((TILE, round(image.height * TILE / image.width)), Image.NEAREST)
    tile = Image.new("RGBA", (TILE, TILE), (0, 0, 0, 0))
    x = {"left": 0, "right": TILE - image.width, "centre": (TILE - image.width) // 2}[align]
    tile.alpha_composite(image, (x, 0))
    return tile


def roof_tiles():
    floor = load("roof_floor")
    floor = floor.crop(ROOF_FLOOR_CROP).resize((TILE, TILE), Image.NEAREST)
    tiles = {"roof_floor": floor}
    for key, (bar_name, side) in ROOF_BARS.items():
        bar = load(bar_name)
        tile = floor.copy()
        horizontal = side in ("top", "bottom")
        thickness = max(1, round((bar.height if horizontal else bar.width) * TILE / 130))
        bar = bar.resize((TILE, thickness) if horizontal else (thickness, TILE), Image.NEAREST)
        at = {"top": (0, 0), "bottom": (0, TILE - thickness), "left": (0, 0), "right": (TILE - thickness, 0)}[side]
        if side == "right":
            bar = bar.transpose(Image.FLIP_LEFT_RIGHT)
        tile.alpha_composite(bar, at)
        tiles["roof_edge_" + key] = tile
    return tiles


def build():
    tiles = {}
    for group, name, src in FLAT:
        tiles["%s_%s" % (group, name)] = inset_square(src, FLAT_INSET)
    for group, name, src in HEDGE:
        tiles["%s_%s" % (group, name)] = inset_square(src, HEDGE_INSET)
    for group, name, src in WALL:
        tiles["%s_%s" % (group, name)] = inset_square(src, WALL_INSET)
    for group, name, src, align in PADDED:
        tiles["%s_%s" % (group, name)] = fitted(src, align)
    for group, name, src, align in AWNINGS:
        tiles["%s_%s" % (group, name)] = awning(src, align)
    tiles.update(roof_tiles())
    return tiles


def seam_score(tile):
    """Mean colour step across the tile's own wrap-around seam divided by the mean step between interior neighbours (about 1 is invisible)."""
    a = np.array(tile.convert("RGB")).astype(float)
    wrap = [np.abs(a[:, 0] - a[:, -1]).mean(), np.abs(a[0] - a[-1]).mean()]
    inner = [np.abs(a[:, 1:] - a[:, :-1]).mean(), np.abs(a[1:] - a[:-1]).mean()]
    return max(wrap) / max(1e-6, max(inner))


def preview(tiles, path):
    cols = 8
    names = sorted(tiles)
    rows = (len(names) + cols - 1) // cols
    sheet = Image.new("RGBA", (cols * (TILE + 4), rows * (TILE + 4)), (255, 0, 255, 255))
    for i, n in enumerate(names):
        sheet.alpha_composite(tiles[n], ((i % cols) * (TILE + 4), (i // cols) * (TILE + 4)))
    sheet.save(os.path.join(path, "tiles_sheet.png"))
    seam = Image.new("RGBA", (TILE * 9, TILE * 3), (255, 0, 255, 255))
    for i, n in enumerate(["ground_asphalt", "ground_asphalt_speckle", "ground_grass", "sidewalk_plain", "hedge_block", "building_wall",
                           "roof_floor", "marking_edge_w", "sidewalk_curb_straight"]):
        t = tiles[n]
        sub = Image.new("RGBA", (TILE * 3, TILE * 3), (255, 0, 255, 255))
        for x in range(3):
            for y in range(3):
                sub.alpha_composite(t, (x * TILE, y * TILE))
        sub.save(os.path.join(path, "seam_%s.png" % n))
        seam.alpha_composite(t, ((i % 9) * TILE, 0))
        seam.alpha_composite(t, ((i % 9) * TILE, TILE))
    seam.save(os.path.join(path, "seam_strip.png"))


def main():
    tiles = build()
    shutil.rmtree(OUT, ignore_errors=True)
    os.makedirs(OUT)
    for name, tile in tiles.items():
        assert tile.size == (TILE, TILE), name
        tile.save(os.path.join(OUT, name + ".png"))
    print("wrote %d tiles of %dx%d to %s" % (len(tiles), TILE, TILE, OUT))
    for name in ("ground_asphalt", "ground_grass", "sidewalk_plain", "roof_floor"):
        print("seam score %s: %.2f" % (name, seam_score(tiles[name])))
    if "--preview" in sys.argv:
        path = sys.argv[sys.argv.index("--preview") + 1]
        os.makedirs(path, exist_ok=True)
        preview(tiles, path)


if __name__ == "__main__":
    main()
