"""Monte Carlo runner and per-cell statistics."""
import math
import os
from dataclasses import replace
from typing import NamedTuple
from concurrent.futures import ProcessPoolExecutor
from night import run_night
from params import SEED
from policies import POLICIES

FIELDS = ("arrivals", "parked", "timeouts", "pickups", "delivered", "on_time", "tips", "util", "peak", "full", "peak_spots")


def percentile(values, q):
    s = sorted(values)
    return s[min(len(s) - 1, math.ceil(q * len(s)) - 1)]


def summarize(rows):
    """rows: list of per-night result tuples (see night.NightSim.result)."""
    n = len(rows)
    col = lambda i: [r[i] for r in rows]
    tot = lambda i: sum(col(i))
    arr = max(1, tot(0))
    return dict(
        arr_mean=tot(0) / n, arr_p95=percentile(col(0), 0.95), arr_max=max(col(0)),
        peak_mean=tot(8) / n, peak_p95=percentile(col(8), 0.95), peak_max=max(col(8)),
        parked_pct=100 * tot(1) / arr, timeout_pct=100 * tot(2) / arr,
        ontime_pct=100 * tot(5) / max(1, tot(3)),
        tips=tot(6) / n, util_pct=100 * tot(7) / n,
        full_mean=tot(9) / n, full_any_pct=100 * sum(1 for r in rows if r[9] > 0) / n,
        spots_p95=percentile(col(10), 0.95), spots_max=max(col(10)), spots_mean=tot(10) / n,
    )


class Job(NamedTuple):
    """One Monte Carlo cell: a night plan, a spot count and a valet policy."""
    plan: object
    spots: int
    policy: str
    runs: int = 500
    rows: int = None          # lot layout rows (default LotConfig: 2)
    stay_scale: float = 1.0   # what-if: multiplies patron stay times
    hands: float = 1.0        # what-if: multiplies every human overhead (0.5 = fast hands)


def _cell(job):
    pol = POLICIES[job.policy]
    pol = replace(pol, mult=pol.mult * job.hands, decide_s=pol.decide_s * job.hands)
    return [run_night(job.plan, job.spots, pol, f"{SEED}-n{job.plan.night}-{i}", job.rows, job.stay_scale)
            for i in range(job.runs)]


def run_cells(jobs, workers=None):
    """Runs every job (in parallel) and returns the raw per-night result rows for each."""
    workers = workers or os.cpu_count() or 1
    if workers == 1 or len(jobs) < 2:
        return [_cell(j) for j in jobs]
    with ProcessPoolExecutor(max_workers=workers) as pool:
        return list(pool.map(_cell, jobs, chunksize=2))
