"""Step 5: the "moderately challenging" bands and the search for the expansion schedule."""
from dataclasses import dataclass
from params import DEFAULT_PLANS
from rules import default_plan
from stats import Job, run_cells, summarize

SCALES = [round(0.4 + 0.1 * i, 1) for i in range(0, 57)]   # arrival gap scale 0.4 .. 6.0
MIN_STEP = 0.1   # each night must be at least one notch busier than the last
SPOT_CANDIDATES = list(range(3, 15))
COMFORT_FULL_PCT = 5   # 'comfortable' lot: full in at most this % of nights
REF_SPOTS = 10   # "ample" lot used while looking for the arrival pace


@dataclass(frozen=True)
class Band:
    """What 'moderate' means on one night, judged on the competent valet."""
    timeout: tuple      # park-timeout % must lie in this range
    util_lo: float      # below this (with timeouts under the range) the night is too easy
    util_hi: float      # above this the night is too hard
    full_hi: float      # max % of nights in which the lot is ever full when a car needs a spot
    target: float = 100.0      # aim for the busiest pace whose competent timeouts stay at or under this %
    expert_hi: float = 100.0   # expert park-timeout % must stay under this


BANDS = {
    1: Band((0, 1), 40, 60, 10, 0.5),
    2: Band((0, 2), 50, 70, 10, 1),
    3: Band((0, 3), 60, 90, 10, 2),
    4: Band((0, 5), 65, 90, 20, 3),
    5: Band((2, 8), 75, 91, 30, 5),
    6: Band((5, 12), 85, 94, 45, 8, 8),
    7: Band((10, 15), 90, 97, 60, 12, 5),
}


# Profile B: "moderate" read as valet utilisation 70-85% on the last night (the brief's other example).
BANDS_B = {
    1: Band((0, 1), 35, 55, 10, 0.5),
    2: Band((0, 1), 45, 62, 10, 0.5),
    3: Band((0, 2), 55, 76, 10, 1),
    4: Band((0, 3), 62, 78, 20, 1.5),
    5: Band((0, 4), 68, 80, 30, 2),
    6: Band((0, 5), 72, 83, 45, 3),
    7: Band((0, 6), 75, 86, 60, 4),
}


def scan_scales(night, cap, runs, workers, hands=1.0, stay=1.0, rows=None, scales=SCALES, ref=REF_SPOTS):
    """Competent + expert summaries at the reference lot for each arrival gap scale."""
    plans = [default_plan(night).with_(cap=cap, gap_scale=s) for s in scales]
    jobs = [Job(p, ref, pol, runs, rows, stay, hands) for p in plans for pol in ("competent", "expert")]
    res = [summarize(r) for r in run_cells(jobs, workers)]
    return [(s, res[2 * i], res[2 * i + 1]) for i, s in enumerate(scales)]


def in_band(band, comp, expert):
    lo, hi = band.timeout
    return (lo <= comp["timeout_pct"] <= min(hi, band.target) and comp["util_pct"] <= band.util_hi
            and expert["timeout_pct"] < band.expert_hi)


def pick_scale(scan, band, max_scale):
    """Busiest pace (lowest gap scale) in band that is no slower than max_scale (the previous night's)."""
    ok = [(s, c, e) for s, c, e in scan if s <= max_scale and in_band(band, c, e)]
    return min(ok, key=lambda t: t[0]) if ok else None


def pick_spots(plan, band, runs, workers, hands=1.0, stay=1.0, rows=None, full_hi=None):
    """Smallest spot count whose lot-full frequency and timeouts fit the band."""
    full_hi = band.full_hi if full_hi is None else full_hi
    jobs = [Job(plan, s, "competent", runs, rows, stay, hands) for s in SPOT_CANDIDATES]
    sums = [summarize(r) for r in run_cells(jobs, workers)]
    for s, r in zip(SPOT_CANDIDATES, sums):
        if r["full_any_pct"] <= full_hi and r["timeout_pct"] <= band.timeout[1]:
            return s, r
    return SPOT_CANDIDATES[-1], sums[-1]


def build_schedule(runs, workers, hands=1.0, stay=1.0, rows=None, caps=None, bands=None):
    """Returns a list of dicts, one per night: plan, spots, and the verification summary of the search."""
    caps = caps or [p[1] for p in DEFAULT_PLANS]
    bands = bands or BANDS
    out, max_scale, prev_spots, prev_comfort = [], max(SCALES), 0, 0
    for n in range(1, 8):
        band = bands[n]
        scan = scan_scales(n, caps[n - 1], runs, workers, hands, stay, rows)
        pick = pick_scale(scan, band, max_scale) or closest(scan, band, max_scale)
        plan = default_plan(n).with_(cap=caps[n - 1], gap_scale=pick[0])
        spots, _ = pick_spots(plan, band, runs, workers, hands, stay, rows)
        spots = max(spots, prev_spots, 4)
        comfort, _ = pick_spots(plan, band, runs, workers, hands, stay, rows, full_hi=COMFORT_FULL_PCT)
        comfort = max(comfort, prev_comfort, spots)
        out.append(dict(night=n, plan=plan, spots=spots, comfort=comfort, band=band))
        max_scale, prev_spots, prev_comfort = round(pick[0] - MIN_STEP, 1), spots, comfort
    return out


def verdict(band, comp):
    """Compare a competent summary with a night's band: too easy / too hard / ok."""
    lo, hi = band.timeout
    if comp["timeout_pct"] > hi or comp["util_pct"] > band.util_hi + 3:
        return "too hard"
    if comp["timeout_pct"] < lo and comp["util_pct"] < band.util_lo:
        return "too easy"
    return "about right"


def closest(scan, band, max_scale):
    """Fallback when nothing is in band: the calmest pace the monotone rule still allows."""
    pool = [t for t in scan if t[0] <= max_scale] or scan
    return max(pool, key=lambda t: t[0])
