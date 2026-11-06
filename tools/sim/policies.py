"""Valet skill policies and action timing (all human timings are ASSUMPTIONS, see params.OVERHEAD_S)."""
import math
from dataclasses import dataclass
from geometry import euclid, manhattan
from params import GAME_MIN_PER_SEC, MOVE, OVERHEAD_S


@dataclass(frozen=True)
class Policy:
    name: str
    mult: float          # scales enter / exit / precision / decide overheads
    spread: float        # +- fraction of random variation on every duration
    order: str           # "fifo" (readiness order) or "edf" (earliest deadline first)
    spot_rule: str       # "random", "nearest" (to the car) or "curb" (nearest the curb)
    drive_frac: float    # fraction of the car's top speed the player actually uses
    decide_s: float      # seconds to look around and pick the next job
    charge_max_wait: int  # skip optional EV charging when more cars than this are waiting
    relocate_slack: float  # EDF: game minutes of slack given to a finished service car


POLICIES = {
    "novice": Policy("novice", 1.5, 0.45, "fifo", "random", 0.60, 1.5, 9, 0.0),
    "competent": Policy("competent", 1.0, 0.30, "edf", "nearest", 0.75, 0.5, 2, 20.0),
    "expert": Policy("expert", 0.7, 0.20, "edf", "curb", 0.90, 0.3, 1, 12.0),
}


def leg_seconds(d, v, a, b):
    """Rest-to-rest travel time over a straight leg: accelerate, cruise at v, brake."""
    if d <= 0:
        return 0.0
    ramp = v * v / (2 * a) + v * v / (2 * b)
    if d >= ramp:
        return v / a + v / b + (d - ramp) / v
    peak = math.sqrt(2 * d / (1 / a + 1 / b))
    return peak / a + peak / b


def drive_seconds(p, q, policy):
    """Manhattan path (two legs, cars stop to turn) at the policy's speed fraction."""
    v = MOVE["car_top"] * policy.drive_frac
    a, b = MOVE["car_accel"], MOVE["car_brake"]
    return leg_seconds(abs(p[0] - q[0]), v, a, b) + leg_seconds(abs(p[1] - q[1]), v, a, b)


def walk_seconds(p, q):
    return max(0.0, euclid(p, q) - MOVE["interact_range"]) / MOVE["walk"]


def jitter(rng, spread):
    return 1.0 + rng.uniform(-spread, spread)


def plan_move(valet_pos, car_pos, dest, precision_key, policy, rng):
    """Seconds of (decide, walk, enter, drive+park precision, exit) for one car move, jittered."""
    j = lambda s: s * jitter(rng, policy.spread)
    o = {k: v * policy.mult for k, v in OVERHEAD_S.items()}
    parts = (
        j(policy.decide_s),
        j(walk_seconds(valet_pos, car_pos)),
        j(o["enter"]),
        j(drive_seconds(car_pos, dest, policy)) + j(o[precision_key]),
        j(o["exit"]),
    )
    return tuple(x * GAME_MIN_PER_SEC for x in parts)  # game minutes


def spot_distance_key(policy, layout, car_pos, spot):
    if policy.spot_rule == "curb":
        return (manhattan(layout.curb, spot), manhattan(car_pos, spot))
    return (manhattan(car_pos, spot),)
