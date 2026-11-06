"""Seven-night runs: total tips per policy against the starting debt."""
from params import STARTING_DEBT
from stats import Job, percentile, run_cells

ORDER = ("novice", "competent", "expert")


def run_totals(plans_and_spots, runs, workers=None, hands=1.0):
    """plans_and_spots: [(Plan, spots)] for nights 1..7. Returns {policy: per-run total tips}."""
    out = {}
    jobs = [Job(plan, s, p, runs, None, 1.0, hands) for p in ORDER for plan, s in plans_and_spots]
    results = run_cells(jobs, workers)
    per = len(plans_and_spots)
    for i, p in enumerate(ORDER):
        nights = results[i * per:(i + 1) * per]
        out[p] = [sum(night[r][6] for night in nights) for r in range(runs)]
    return out


def totals_table(totals, title):
    head = ["| " + title + " | mean tips / run | p10 | p90 | runs that clear the $%d debt |" % STARTING_DEBT,
            "| --- | --- | --- | --- | --- |"]
    for p, vals in totals.items():
        n = len(vals)
        paid = 100 * sum(1 for v in vals if v >= STARTING_DEBT) / n
        head.append(f"| {p} | ${sum(vals) / n:.0f} | ${percentile(vals, 0.10)} | ${percentile(vals, 0.90)} | {paid:.0f}% |")
    return "\n".join(head)
