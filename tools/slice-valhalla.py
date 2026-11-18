"""Slices the Valhalla Drive source art (art-src/ValhallaDrive) into individual sprites for the sandbox build.

Repeatable: run `python tools/slice-valhalla.py` from the repository root (needs Pillow and numpy). It writes
Assets/_Project/Art/ValhallaMidgard/{Lot,Thor,Npc}/*.png and manifest.json, replacing what was there.

Cleaning: the AI sheets carry a halo of semi-transparent red and yellow pixels around the sprites. Pixels under the alpha
threshold are dropped, the rest are made fully opaque (a hard pixel edge), specks that are not part of a sprite are removed.
"""
import json
import os
import shutil
import sys
from collections import deque

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC = os.path.join(ROOT, "art-src", "ValhallaDrive")
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "ValhallaMidgard")

ALPHA_KEEP = 128      # pixels with less alpha are halo and are dropped
MIN_AREA = 30         # smaller connected pieces are specks
GROUP_DILATE = 2      # pixels bridged when grouping pieces of one sprite (awnings, signs, thin poles)
SPECK_SHARE = 0.04    # in a walk frame, pieces under this share of the largest are specks

# Row bands of the tileset by the vertical centre of a sprite; each row is named left to right.
TILE_ROWS = [
    (0, 150, ["asphalt_plain", "asphalt_speckle", "asphalt_cracked", "asphalt_skid", "edge_line_w", "edge_line_e", "edge_line_n",
              "edge_line_s", "bay_empty", "bay_p", "bay_handicap"]),
    (150, 270, ["crosswalk_h", "crosswalk_v", "crosswalk_v_thin", "crosswalk_stagger", "centre_line_h", "centre_line_v",
                "road_curb_straight", "road_curb_corner_outer", "road_curb_corner_inner", "road_curb_t", "road_curb_cross"]),
    (270, 400, ["sidewalk_plain", "sidewalk_corner_a", "sidewalk_corner_b", "sidewalk_t", "sidewalk_cross", "grass", "hedge_block",
                "hedge_corner_a", "hedge_corner_b", "hedge_t", "hedge_cross"]),
    (400, 545, ["wall_pilaster_left", "wall_window_1", "wall_window_2", "wall_window_3", "wall_window_4", "wall_pilaster_right",
                "roof_plain", "roof_vent_single", "roof_vent_double", "roof_pipe"]),
    (545, 650, ["wall_door_double", "wall_plain", "wall_door_single", "wall_corner", "awning_left", "awning_mid_a", "awning_mid_b",
                "awning_right", "flowerbox_large", "flowerbox_medium", "flowerbox_small"]),
    (650, 800, ["diner_sign", "diner_sign_vertical", "gadget_dome", "gadget_hvac_a", "gadget_hvac_b", "gadget_panel", "gadget_dish"]),
    (800, 940, ["pad_drop_off", "pad_valet_pickup_left", "pad_valet_pickup_right", "bay_dashed_a", "bay_dashed_b", "bay_1", "bay_2",
                "bay_3", "bay_4", "bay_5", "bay_6", "bay_7", "boom_barrier", "open_sign"]),
    (940, 1060, ["tree_large", "tree_small", "bush", "planter_flowers", "hedge_planter", "bench", "lamp_post", "bin", "hydrant",
                 "meter", "fence", "bollard", "cone", "mailbox", "newspaper_box", "sign_coffee"]),
    (1060, 2000, ["car_red", "car_blue", "car_yellow", "car_green", "car_black", "car_white", "van_white", "portrait_chef",
                  "portrait_bellhop", "portrait_brown_man", "portrait_orange_girl", "portrait_black_girl", "portrait_grandma",
                  "portrait_blue_cap_man", "portrait_blonde_girl"]),
]

NPC_LOOKS = ["grandma", "chef", "builder", "brown_man", "orange_girl", "black_girl", "blue_cap_man", "flat_cap_man"]
DIRECTIONS_NPC = ["down", "left", "right", "up"]   # four groups of four frames along a row
DIRECTIONS_THOR = ["down", "left", "right", "up"]  # one row per direction
FRAMES = 4


def components(mask):
    """8-connected components of a boolean mask as (x0, y0, x1, y1, pixel-index array)."""
    h, w = mask.shape
    seen = np.zeros((h, w), bool)
    found = []
    for y0, x0 in zip(*np.nonzero(mask)):
        if seen[y0, x0]:
            continue
        seen[y0, x0] = True
        queue = deque([(y0, x0)])
        pts = []
        while queue:
            y, x = queue.popleft()
            pts.append((y, x))
            for dy in (-1, 0, 1):
                for dx in (-1, 0, 1):
                    yy, xx = y + dy, x + dx
                    if 0 <= yy < h and 0 <= xx < w and mask[yy, xx] and not seen[yy, xx]:
                        seen[yy, xx] = True
                        queue.append((yy, xx))
        p = np.array(pts)
        found.append((p[:, 1].min(), p[:, 0].min(), p[:, 1].max() + 1, p[:, 0].max() + 1, p))
    return found


def dilate(mask, times):
    d = mask.copy()
    h, w = mask.shape
    for _ in range(times):
        p = np.pad(d, 1)
        d = np.zeros_like(d)
        for dy in range(3):
            for dx in range(3):
                d |= p[dy:dy + h, dx:dx + w]
    return d


def hard_edge(rgba):
    """Drops the halo (alpha under the threshold) and makes the rest opaque; transparent pixels get black RGB."""
    out = rgba.copy()
    keep = out[..., 3] >= ALPHA_KEEP
    out[..., 3] = np.where(keep, 255, 0)
    out[~keep, :3] = 0
    return out


def slice_tileset(image, manifest):
    cleaned = hard_edge(image)
    keep = cleaned[..., 3] > 0
    groups = [c for c in components(dilate(keep, GROUP_DILATE)) if len(c[4]) > MIN_AREA]
    rows = {i: [] for i in range(len(TILE_ROWS))}
    for g in groups:
        centre = (g[1] + g[3]) / 2
        for i, (lo, hi, _) in enumerate(TILE_ROWS):
            if lo <= centre < hi:
                rows[i].append(g)
    folder = os.path.join(OUT, "Lot")
    os.makedirs(folder, exist_ok=True)
    for i, (_, _, names) in enumerate(TILE_ROWS):
        row = sorted(rows[i], key=lambda g: g[0])
        if len(row) != len(names):
            sys.exit("tileset row %d has %d sprites, expected %d" % (i, len(row), len(names)))
        for name, (x0, y0, x1, y1, pts) in zip(names, row):
            region = np.zeros((y1 - y0, x1 - x0, 4), np.uint8)
            sub = cleaned[y0:y1, x0:x1]
            inside = np.zeros((y1 - y0, x1 - x0), bool)
            inside[pts[:, 0] - y0, pts[:, 1] - x0] = True
            sub_keep = (sub[..., 3] > 0) & dilate(inside, 0)
            region[sub_keep] = sub[sub_keep]
            trim_save(region, os.path.join(folder, name + ".png"), manifest, "lot", name, (x0, y0))


def crop_save(source_name, box, name, manifest):
    """A derived sprite: a pixel box (x0, y0, x1, y1) of an already cut sprite, e.g. the roof floor without its frame."""
    folder = os.path.join(OUT, "Lot")
    image = Image.open(os.path.join(folder, source_name + ".png")).convert("RGBA").crop(box)
    path = os.path.join(folder, name + ".png")
    image.save(path)
    manifest.append({"kind": "lot", "name": name, "file": os.path.relpath(path, OUT).replace("\\", "/"),
                     "width": image.width, "height": image.height, "derived": source_name})


def make_bay_8(manifest):
    """The sheet has bays 1 to 7 only. Bay 8 is bay 6 with its numeral wiped and an 8 drawn in the same light colour."""
    from PIL import ImageDraw, ImageFont
    folder = os.path.join(OUT, "Lot")
    image = Image.open(os.path.join(folder, "bay_6.png")).convert("RGBA")
    a = np.array(image)
    margin = 10
    inner = a[margin:-margin, margin:-margin]
    light = inner[..., :3].mean(axis=2) > 140
    ys, xs = np.nonzero(light)
    fill = np.median(inner[~light][:, :3], axis=0).astype(np.uint8)
    ink = np.median(inner[light][:, :3], axis=0).astype(np.uint8)
    top, bottom = ys.min() + margin, ys.max() + margin + 1
    left, right = xs.min() + margin, xs.max() + margin + 1
    a[top - 3:bottom + 3, left - 3:right + 3, :3] = fill
    glyph_h = bottom - top
    font = None
    for candidate in ("arialbd.ttf", "arial.ttf", "DejaVuSans-Bold.ttf"):
        try:
            font = ImageFont.truetype(candidate, int(glyph_h * 1.38))
            break
        except OSError:
            continue
    if font is None:
        sys.exit("no TrueType font found for bay_8")
    canvas = Image.fromarray(a, "RGBA")
    box = ImageDraw.Draw(canvas).textbbox((0, 0), "8", font=font)
    centre_x, centre_y = (left + right) / 2, (top + bottom) / 2
    at = (centre_x - (box[0] + box[2]) / 2, centre_y - (box[1] + box[3]) / 2)
    ImageDraw.Draw(canvas).text(at, "8", font=font, fill=tuple(int(v) for v in ink) + (255,))
    path = os.path.join(folder, "bay_8.png")
    canvas.save(path)
    manifest.append({"kind": "lot", "name": "bay_8", "file": "Lot/bay_8.png", "width": canvas.width, "height": canvas.height,
                     "derived": "bay_6"})


def derive_roof(manifest):
    """The roof tile is a floor with a tan frame on three sides; the floor and the frame bars are cut out so any roof size can be built."""
    crop_save("roof_plain", (14, 11, 144, 92), "roof_floor", manifest)
    crop_save("roof_plain", (14, 0, 144, 10), "roof_frame_top", manifest)
    crop_save("roof_plain", (14, 94, 144, 112), "roof_frame_bottom", manifest)
    crop_save("roof_plain", (0, 0, 13, 112), "roof_frame_left", manifest)


def trim_save(region, path, manifest, kind, name, origin):
    ys, xs = np.nonzero(region[..., 3])
    region = region[ys.min():ys.max() + 1, xs.min():xs.max() + 1]
    Image.fromarray(region, "RGBA").save(path)
    manifest.append({"kind": kind, "name": name, "file": os.path.relpath(path, OUT).replace("\\", "/"),
                     "width": int(region.shape[1]), "height": int(region.shape[0]),
                     "source": [int(origin[0]), int(origin[1])]})


def frame_cells(image, columns, rows):
    """Cleaned cell images of a grid sheet with specks removed (only the sprite's own pieces stay)."""
    cleaned = hard_edge(image)
    h, w = cleaned.shape[:2]
    cells = {}
    for r in range(rows):
        for c in range(columns):
            x0, x1 = round(c * w / columns), round((c + 1) * w / columns)
            y0, y1 = round(r * h / rows), round((r + 1) * h / rows)
            cell = cleaned[y0:y1, x0:x1].copy()
            pieces = components(cell[..., 3] > 0)
            if not pieces:
                sys.exit("empty frame at row %d column %d" % (r, c))
            biggest = max(len(p[4]) for p in pieces)
            keep = np.zeros(cell.shape[:2], bool)
            for p in pieces:
                if len(p[4]) >= biggest * SPECK_SHARE:
                    keep[p[4][:, 0], p[4][:, 1]] = True
            cell[~keep] = 0
            cells[(r, c)] = cell
    return cells


def save_frames(cells, name_of, folder, manifest, kind):
    """Every frame is cropped to the union box of the whole set, so the frames stay aligned (feet at the bottom)."""
    boxes = []
    for cell in cells.values():
        ys, xs = np.nonzero(cell[..., 3])
        boxes.append((xs.min(), ys.min(), xs.max() + 1, ys.max() + 1))
    x0, y0 = min(b[0] for b in boxes), min(b[1] for b in boxes)
    x1, y1 = max(b[2] for b in boxes), max(b[3] for b in boxes)
    os.makedirs(folder, exist_ok=True)
    for key, cell in cells.items():
        name = name_of(key)
        path = os.path.join(folder, name + ".png")
        Image.fromarray(cell[y0:y1, x0:x1], "RGBA").save(path)
        manifest.append({"kind": kind, "name": name, "file": os.path.relpath(path, OUT).replace("\\", "/"),
                         "width": int(x1 - x0), "height": int(y1 - y0)})


def slice_thor(image, manifest):
    cells = frame_cells(image, FRAMES, len(DIRECTIONS_THOR))
    save_frames(cells, lambda k: "thor_%s_%d" % (DIRECTIONS_THOR[k[0]], k[1]), os.path.join(OUT, "Thor"), manifest, "thor")


def slice_npcs(image, manifest):
    cells = frame_cells(image, FRAMES * len(DIRECTIONS_NPC), len(NPC_LOOKS))

    def name_of(k):
        return "npc_%s_%s_%d" % (NPC_LOOKS[k[0]], DIRECTIONS_NPC[k[1] // FRAMES], k[1] % FRAMES)

    save_frames(cells, name_of, os.path.join(OUT, "Npc"), manifest, "npc")


def main():
    for sub in ("Lot", "Thor", "Npc"):
        shutil.rmtree(os.path.join(OUT, sub), ignore_errors=True)
    manifest = []
    load = lambda f: np.array(Image.open(os.path.join(SRC, f)).convert("RGBA"))
    slice_tileset(load("midgard-diner-tileset.png"), manifest)
    make_bay_8(manifest)
    derive_roof(manifest)
    slice_thor(load("thor-walk-sheet.png"), manifest)
    slice_npcs(load("midgard-npc-walk-sheet.png"), manifest)
    with open(os.path.join(OUT, "manifest.json"), "w") as f:
        json.dump({"sprites": manifest}, f, indent=1)
    print("wrote %d sprites to %s" % (len(manifest), OUT))


if __name__ == "__main__":
    main()
