"""One night, minute by minute: patrons, tickets, stations, spots and one valet.

Game rules run on whole-minute ticks exactly like NightClock (TicketRegistry first, then the
spawner). The valet works in continuous time between ticks.
"""
import math
import random
from car import Car
from geometry import build_layout
from params import LOT, NIGHT_MIN, PATRON
from rules import next_gap, roll_car
from tasks import SETTLE, TaskMixin
from valet import Valet, decide

EV_TICKS = math.ceil(round((1 - PATRON["ev_start"]) * PATRON["charge_min"], 6))
WASH_TICKS = math.ceil(PATRON["wash_min"])


class NightSim(TaskMixin):
    def __init__(self, plan, n_spots, policy, arr_rng, val_rng, rows=None, stay_scale=1.0):
        self.plan, self.policy, self.arng, self.vrng = plan, policy, arr_rng, val_rng
        self.stay_scale = stay_scale
        self.nslots = max(plan.cap, LOT["queue_slots"])
        self.layout = build_layout(n_spots, self.nslots, rows)
        self.spots = [None] * n_spots
        self.bays = [None] * (LOT["charging_bays"] if plan.charging else 0)
        self.wash_car = None
        self.cars, self.next_id, self.next_arrival = [], 0, 0.0
        self.valet = Valet(self.layout.start)
        self.arrivals = self.parked = self.timeouts = self.pickups = 0
        self.full_ids = set()
        self.delivered = self.on_time = self.paid = self.tips = self.peak = self.peak_spots = 0

    # ---- queries ------------------------------------------------------------------------
    def free_spot_ids(self):
        return [i for i, c in enumerate(self.spots) if c is None]

    def awaiting_count(self):
        return sum(1 for c in self.cars if c.state in ("queue", "bay"))

    def waiting_count(self):
        return sum(1 for c in self.cars if c.state == "queue")

    def next_job(self, car):
        """What the valet could do with this car right now (None = nothing)."""
        if car.state == "pickup":
            return "deliver"
        if car.state == "bay":
            return "relocate" if car.service_left <= 0 and None in self.spots else None
        if car.state != "queue":
            return None
        return self._route(car)

    def _route(self, car):
        if car.dirty:
            return "wash" if self.plan.wash and self.wash_car is None else None
        wanted = car.ev and self.plan.charging and None in self.bays
        if wanted and self.waiting_count() <= self.policy.charge_max_wait:
            return "charge"
        return "park" if None in self.spots else None

    # ---- ticks --------------------------------------------------------------------------
    def on_tick(self, m):
        for car in list(self.cars):
            self._step_car(car, m)
        self._spawn(m)
        self.peak = max(self.peak, len(self.cars))
        self.peak_spots = max(self.peak_spots, sum(1 for c in self.spots if c is not None))
        if None not in self.spots:
            for car in self.cars:
                if car.state == "queue" and not car.dirty:
                    self.full_ids.add(car.id)

    def _step_car(self, car, m):
        if car.state == "queue":
            if car.park_elapsed(m) >= PATRON["park_window"]:
                self._timeout(car, m)
        elif car.state == "bay":
            if m > car.bay_in and car.service_left > 0:
                car.service_left -= 1
                car.ready = m if car.service_left <= 0 else car.ready
        elif car.state == "spot":
            if m >= car.parked_min + car.stay:
                car.state, car.req = "pickup", float(m)
                self.pickups += 1
        elif car.state == "pickup" and m - car.req >= PATRON["pickup_window"]:
            car.expired = True

    def _timeout(self, car, m):
        task = self.valet.task
        if car.claimed and task is not None and task.car is car:
            self._abort(task, m)
        self.timeouts += 1
        self.cars.remove(car)

    def _spawn(self, m):
        if m < self.next_arrival:
            return
        stay, imp, ev, dirty = roll_car(self.arng, self.plan)
        if self.awaiting_count() >= self.plan.cap:
            return
        used = {c.slot for c in self.cars if c.holds_slot(m)}
        free = [i for i in range(min(self.plan.cap, self.nslots)) if i not in used]
        if not free:
            return
        slot = free[0]
        car = Car(self.next_id, float(m), stay * self.stay_scale, imp, ev, dirty, slot,
                  self.layout.queue[slot], resume=float(m))
        car.service_left = EV_TICKS if ev else (WASH_TICKS if dirty else 0)
        self.next_id += 1
        self.arrivals += 1
        self.cars.append(car)
        self.next_arrival = m + next_gap(self.arng, self.plan)

    # ---- main loop ----------------------------------------------------------------------
    def run(self):
        m = 0
        while m < NIGHT_MIN:
            task = self.valet.task
            if task is not None and task.end <= m + 1:
                self.complete(task)
                decide(self, task.end)
                continue
            m += 1
            self.on_tick(m)
            if self.valet.task is None:
                decide(self, float(m))
        if self.valet.task is not None:
            self.valet.busy += NIGHT_MIN - self.valet.task.start
        return self.result()

    def result(self):
        return (self.arrivals, self.parked, self.timeouts, self.pickups, self.delivered,
                self.on_time, self.tips, self.valet.busy / NIGHT_MIN, self.peak, len(self.full_ids), self.peak_spots)


def run_night(plan, n_spots, policy, seed, rows=None, stay_scale=1.0):
    """Runs one seeded night. Arrival randomness is shared by all policies for a given seed."""
    arr = random.Random(f"{seed}-a")
    val = random.Random(f"{seed}-v-{policy.name}")
    return NightSim(plan, n_spots, policy, arr, val, rows, stay_scale).run()
