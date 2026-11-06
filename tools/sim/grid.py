"""Step 4: the full night x spots x policy grid, CSV export and markdown tables."""
import csv
from params import SPOT_RANGE
from policies import POLICIES
from rules import default_plan
from stats import Job, run_cells, summarize

ORDER = ("novice", "competent", "expert")


def run_grid(runs, workers=None, hands=1.0, spots=SPOT_RANGE, nights=range(1, 8)):
    """Returns {(night, spots, policy): summary} for NightPlanDefaults at every spot count."""
    keys = [(n, s, p) for n in nights for s in spots for p in ORDER]
    jobs = [Job(default_plan(n), s, p, runs, None, 1.0, hands) for n, s, p in keys]
    return {k: summarize(rows) for k, rows in zip(keys, run_cells(jobs, workers))}


def write_csv(grid, path):
    with open(path, "w", newline="") as f:
        w = csv.writer(f)
        first = next(iter(grid.values()))
        w.writerow(["night", "spots", "policy"] + list(first))
        for (n, s, p), r in sorted(grid.items()):
            w.writerow([n, s, p] + [round(v, 3) for v in r.values()])


def _trio(grid, n, s, key, fmt="{:.0f}"):
    return "/".join(fmt.format(grid[(n, s, p)][key]) for p in ORDER)


def night_table(grid, n, spots=SPOT_RANGE):
    """Markdown table for one night. Triples are novice/competent/expert."""
    head = ("| S | cars/night mean, p95, max | cars in lot mean, p95, max | spots used p95, max "
            "| parked % n/c/e | on-time pickups % n/c/e | timeouts % n/c/e | tips $ n/c/e "
            "| valet util % n/c/e | lot-full cars/night n/c/e |")
    lines = [head, "|" + " --- |" * 10]
    for s in spots:
        c = grid[(n, s, "competent")]
        lines.append(
            f"| {s} | {c['arr_mean']:.1f}, {c['arr_p95']}, {c['arr_max']} "
            f"| {c['peak_mean']:.1f}, {c['peak_p95']}, {c['peak_max']} "
            f"| {c['spots_p95']}, {c['spots_max']} "
            f"| {_trio(grid, n, s, 'parked_pct')} | {_trio(grid, n, s, 'ontime_pct')} "
            f"| {_trio(grid, n, s, 'timeout_pct')} | {_trio(grid, n, s, 'tips')} "
            f"| {_trio(grid, n, s, 'util_pct')} | {_trio(grid, n, s, 'full_mean', '{:.1f}')} |")
    return "\n".join(lines)


def all_tables(grid, nights=range(1, 8)):
    return "\n\n".join(f"### Night {n} (current NightPlanDefaults)\n\n{night_table(grid, n)}" for n in nights)


def policy_table():
    rows = ["| policy | overhead x | spread +- | order | spot choice | drive speed | decide s |",
            "| --- | --- | --- | --- | --- | --- | --- |"]
    for p in POLICIES.values():
        rows.append(f"| {p.name} | {p.mult} | {p.spread} | {p.order} | {p.spot_rule} "
                    f"| {p.drive_frac:.0%} of top | {p.decide_s} |")
    return "\n".join(rows)
