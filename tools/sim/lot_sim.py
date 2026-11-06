"""Uncanny Valet lot-count balance simulation.

Usage (plain Python 3, standard library only, fixed seed, so output is reproducible):
    python tools/sim/lot_sim.py                    # everything, 500 nights per cell (a few minutes)
    python tools/sim/lot_sim.py --quick            # 100 nights per cell, for a fast look
    python tools/sim/lot_sim.py --sections grid   # grid | runs | schedule | sens (comma separated)
    python tools/sim/lot_sim.py --hands 0.5        # what-if: every human overhead halved

Writes the printed text to tools/sim/results/report.md and the grid to tools/sim/results/grid.csv.
See docs/balance.md for the method and every assumption.
"""
import argparse
import os
import sys
import time
from dataclasses import replace

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import grid as G                    # noqa: E402
import runs as R                    # noqa: E402
import schedule as S                # noqa: E402
from params import CURRENT_SPOTS, DEFAULT_PLANS, RUNS, SEED, STARTING_DEBT   # noqa: E402
from rules import default_plan      # noqa: E402
from stats import Job, run_cells, summarize   # noqa: E402

OUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "results")
LINES = []


def emit(text=""):
    print(text, flush=True)
    LINES.append(text)


def section_grid(a):
    grid = G.run_grid(a.runs, a.workers, a.hands)
    os.makedirs(OUT_DIR, exist_ok=True)
    G.write_csv(grid, os.path.join(OUT_DIR, "grid.csv"))
    emit("## Grid: current NightPlanDefaults, S = 4..24, three valet policies\n")
    emit(G.all_tables(grid))
    emit("\n## Verdict of the CURRENT config (S = %d) per night, competent valet\n" % CURRENT_SPOTS)
    emit("| night | timeouts % | util % | verdict |\n| --- | --- | --- | --- |")
    for n in range(1, 8):
        c = grid[(n, CURRENT_SPOTS, "competent")]
        emit(f"| {n} | {c['timeout_pct']:.0f} | {c['util_pct']:.0f} | {S.verdict(S.BANDS[n], c)} |")


def current_plans():
    return [(default_plan(n), CURRENT_SPOTS) for n in range(1, 8)]


def schedule_row(item, runs, workers, hands):
    """Novice / competent / expert at the scheduled lot, plus competent in an ample (24-spot) lot."""
    jobs = [Job(item["plan"], item["spots"], p, runs, None, 1.0, hands) for p in G.ORDER]
    jobs.append(Job(item["plan"], 24, "competent", runs, None, 1.0, hands))
    return [summarize(r) for r in run_cells(jobs, workers)]


def schedule_table(title, bands, a):
    items = S.build_schedule(a.runs, a.workers, a.hands, bands=bands)
    emit("## %s (verified with %d nights per cell)\n" % (title, a.runs))
    emit("| night | spots | added | comfort spots | MaxWaitingCars | gap scale | cars/night mean, p95, max "
         "| spots demanded p95, max | cars in lot p95, max | timeouts % n/c/e | on-time % c | tips $ n/c/e "
         "| util % c | lot-full nights % c |")
    emit("|" + " --- |" * 14)
    prev = 0
    for it in items:
        nov, c, e, amp = schedule_row(it, a.runs, a.workers, a.hands)
        p = it["plan"]
        emit(f"| {p.night} | {it['spots']} | {'+%d' % (it['spots'] - prev) if prev else 'start'} | {it['comfort']} | {p.cap} | {p.gap_scale} "
             f"| {c['arr_mean']:.1f}, {c['arr_p95']}, {c['arr_max']} | {amp['spots_p95']}, {amp['spots_max']} "
             f"| {amp['peak_p95']}, {amp['peak_max']} | {nov['timeout_pct']:.0f}/{c['timeout_pct']:.0f}/{e['timeout_pct']:.0f} "
             f"| {c['ontime_pct']:.0f} | {nov['tips']:.0f}/{c['tips']:.0f}/{e['tips']:.0f} | {c['util_pct']:.0f} "
             f"| {c['full_any_pct']:.0f} |")
        prev = it["spots"]
    emit("\nBands (competent valet): " + "; ".join(
        f"N{n}: timeouts {b.timeout[0]}-{b.timeout[1]}% (aim <= {b.target}), util <= {b.util_hi:.0f}%, lot full in <= {b.full_hi:.0f}% of nights"
        + (f", expert < {b.expert_hi:.0f}%" if b.expert_hi < 100 else "") for n, b in bands.items()))
    totals = R.run_totals([(it["plan"], it["spots"]) for it in items], a.runs, a.workers, a.hands)
    emit("\n" + R.totals_table(totals, "Seven-night total tips"))


def section_schedule(a):
    schedule_table("Schedule A: moderate as in the brief (competent timeouts 10-15% by night 7)", S.BANDS, a)
    emit()
    schedule_table("Schedule B: gentler (competent valet utilisation 75-86% by night 7)", S.BANDS_B, a)


def section_runs(a):
    emit("## Seven-night totals vs starting debt $%d\n" % STARTING_DEBT)
    totals = R.run_totals(current_plans(), a.runs, a.workers, a.hands)
    emit(R.totals_table(totals, "CURRENT config (S=%d, NightPlanDefaults)" % CURRENT_SPOTS))


def section_sens(a):
    emit("## Sensitivity: night-7 pace and lot size when an assumption changes (competent valet)\n")
    emit("Band N7 without the expert condition (the expert/competent gap itself depends on the assumptions).\n")
    emit("| variant | gap scale | cars/night mean | spots (tight) | spots demanded p95 | timeouts % | util % | tips $/night |")
    emit("| --- | --- | --- | --- | --- | --- | --- | --- |")
    band = replace(S.BANDS[7], expert_hi=100.0, target=15)
    variants = [("base", 1.0, 1.0, None), ("fast hands (overheads x0.5)", 0.5, 1.0, None),
                ("slow hands (overheads x1.5)", 1.5, 1.0, None), ("stays x1.5", 1.0, 1.5, None),
                ("stays x2", 1.0, 2.0, None), ("3-row lot layout", 1.0, 1.0, 3)]
    runs = max(100, a.runs // 2)
    for name, hands, stay, rows in variants:
        h = hands * a.hands
        scan = S.scan_scales(7, 3, runs, a.workers, h, stay, rows, ref=24)
        pick = S.pick_scale(scan, band, 99) or S.closest(scan, band, 99)
        plan = default_plan(7).with_(cap=3, gap_scale=pick[0])
        spots, r = S.pick_spots(plan, band, runs, a.workers, h, stay, rows)
        amp = pick[1]
        flag = "" if S.in_band(band, pick[1], pick[2]) else " (closest, band not reachable)"
        emit(f"| {name}{flag} | {pick[0]} | {r['arr_mean']:.1f} | {spots} | {amp['spots_p95']} "
             f"| {r['timeout_pct']:.0f} | {r['util_pct']:.0f} | {r['tips']:.0f} |")


SECTIONS = {"grid": section_grid, "runs": section_runs, "schedule": section_schedule, "sens": section_sens}


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--runs", type=int, default=RUNS, help="Monte Carlo nights per cell (default 500)")
    ap.add_argument("--quick", action="store_true", help="100 nights per cell")
    ap.add_argument("--sections", default="grid,runs,schedule,sens")
    ap.add_argument("--hands", type=float, default=1.0, help="multiplies every human overhead")
    ap.add_argument("--workers", type=int, default=None, help="processes (default: all cores)")
    a = ap.parse_args()
    a.runs = 100 if a.quick else a.runs
    emit(f"# Lot balance simulation  (seed {SEED}, {a.runs} nights per cell, hands x{a.hands})\n")
    emit(G.policy_table() + "\n")
    t0 = time.time()
    for name in a.sections.split(","):
        SECTIONS[name.strip()](a)
        emit()
    emit(f"(done in {time.time() - t0:.0f} s)")
    os.makedirs(OUT_DIR, exist_ok=True)
    with open(os.path.join(OUT_DIR, "report.md"), "w") as f:
        f.write("\n".join(LINES) + "\n")


if __name__ == "__main__":
    main()
