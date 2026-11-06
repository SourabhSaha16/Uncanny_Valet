"""Lot layout in local lot coordinates: a port of Lot/LotGeometry.cs, extended to any spot count.

Spots fill column by column (all rows of a column, then the next column to the right), so the
first S spots are always the ones nearest the curb. S = 12 with rows = 2 reproduces LotConfig.
"""
import math
from dataclasses import dataclass
from params import LOT


@dataclass(frozen=True)
class Layout:
    spots: tuple    # centres, nearest the curb first
    curb: tuple
    queue: tuple    # waiting-car slot centres, slot 0 is the arrival point
    charge: tuple   # charging bay centres
    wash: tuple     # the single wash slot
    start: tuple    # valet start


def _spot_centres(n, rows, lot):
    sx, sy = lot["spot"]
    left = lot["curb_pos"][0] + lot["curb_size"][0] / 2 + lot["curb_to_spots"]
    top = (rows * sy + (rows - 1) * lot["aisle"]) / 2
    out = []
    for k in range(n):
        col, row = divmod(k, rows)
        out.append((left + sx / 2 + col * (sx + lot["spot_gap"]),
                    top - sy / 2 - row * (sy + lot["aisle"])))
    return tuple(out)


def _queue_slots(n, lot):
    cx, cy = lot["curb_pos"]
    nose_to_centre = lot["queue_car"][1] / 2
    first_y = cy - lot["curb_size"][1] / 2 - lot["queue_start_gap"] - nose_to_centre
    pitch = lot["queue_car"][0] + lot["queue_gap"]
    return tuple((cx - pitch * i, first_y) for i in range(n))


def _stations(lot):
    pitch = lot["station"][0] + lot["station_gap"]
    fx, fy = lot["stations_first"]
    cells = [(fx + pitch * i, fy) for i in range(lot["charging_bays"] + 1)]
    return tuple(cells[:-1]), cells[-1]


def build_layout(n_spots, queue_slots, rows=None, lot=LOT):
    rows = rows or lot["rows"]
    charge, wash = _stations(lot)
    curb = tuple(lot["curb_pos"])
    start = (curb[0] + lot["curb_size"][0] / 2 + lot["valet_start_offset"], curb[1])
    return Layout(_spot_centres(n_spots, rows, lot), curb,
                  _queue_slots(max(1, queue_slots), lot), charge, wash, start)


def manhattan(a, b):
    return abs(a[0] - b[0]) + abs(a[1] - b[1])


def euclid(a, b):
    return math.hypot(a[0] - b[0], a[1] - b[1])
