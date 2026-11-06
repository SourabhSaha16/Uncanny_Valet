"""The single valet: picks the next job by policy and times it from real lot distances."""
from dataclasses import dataclass
from params import PATRON
from policies import plan_move, spot_distance_key


@dataclass(eq=False)
class Task:
    kind: str       # park | charge | wash | relocate | deliver
    car: object
    start: float
    t_in: float     # minute the valet gets into the car
    end: float      # minute the valet has exited and is free
    spot: int = -1
    bay: int = -1


class Valet:
    def __init__(self, pos):
        self.pos = pos
        self.task = None
        self.busy = 0.0


def _key(sim, car, kind, t):
    """Sort key: smaller = sooner."""
    p = sim.policy
    if p.order == "fifo":
        return car.arrive if kind in ("park", "charge", "wash") else (car.ready if kind == "relocate" else car.req)
    if kind in ("park", "charge", "wash"):
        return t + PATRON["park_window"] - car.park_elapsed(t)
    if kind == "relocate":
        return t if sim.awaiting_count() >= sim.plan.cap else t + p.relocate_slack
    return car.req + PATRON["pickup_window"]


def _choose_spot(sim, car):
    free = sim.free_spot_ids()
    if sim.policy.spot_rule == "random":
        return sim.vrng.choice(free)
    lay = sim.layout
    return min(free, key=lambda i: spot_distance_key(sim.policy, lay, car.pos, lay.spots[i]))


def _destination(sim, kind, car):
    """Returns (dest position, precision key, spot index, bay index)."""
    lay = sim.layout
    if kind in ("park", "relocate"):
        spot = _choose_spot(sim, car)
        return lay.spots[spot], "park_precision", spot, -1
    if kind == "charge":
        bay = sim.bays.index(None)
        return lay.charge[bay], "station_precision", -1, bay
    if kind == "wash":
        return lay.wash, "station_precision", -1, -1
    return lay.curb, "curb_precision", -1, -1


def start_task(sim, t, kind, car):
    dest, prec, spot, bay = _destination(sim, kind, car)
    decide, walk, enter, drive, exit_ = plan_move(sim.valet.pos, car.pos, dest, prec, sim.policy, sim.vrng)
    t_in = t + decide + walk + enter
    task = Task(kind, car, t, t_in, t_in + drive + exit_, spot, bay)
    sim.reserve(task)
    sim.valet.task = task
    return task


def decide(sim, t):
    """Pick and start the best available job at minute t, or stay idle (returns None)."""
    best = None
    for car in sim.cars:
        kind = None if car.claimed else sim.next_job(car)
        if kind is None:
            continue
        k = (_key(sim, car, kind, t), car.id)
        if best is None or k < best[0]:
            best = (k, kind, car)
    return start_task(sim, t, best[1], best[2]) if best else None
