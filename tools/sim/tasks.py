"""Valet task bookkeeping for NightSim: reservations, completion effects and aborts."""
from params import GAME_MIN_PER_SEC, MOVE
from rules import tip

SETTLE = MOVE["settle_s"] * GAME_MIN_PER_SEC  # stop-and-settle time before a zone fires, game min


class TaskMixin:
    def reserve(self, task):
        car = task.car
        car.claimed = True
        if task.spot >= 0:
            self.spots[task.spot] = car
            car.spot_index = task.spot
        if task.kind == "charge":
            self.bays[task.bay] = car
            car.station = ("c", task.bay)
        elif task.kind == "wash":
            self.wash_car = car
            car.station = ("w", 0)
        if task.kind in ("park", "charge", "wash"):
            car.pause_at = task.t_in

    def _free_station(self, car):
        if car.station and car.station[0] == "c":
            self.bays[car.station[1]] = None
        elif car.station:
            self.wash_car = None
        car.station = None

    def complete(self, task):
        car, end = task.car, task.end
        self.valet.busy += end - task.start
        self.valet.pos, self.valet.task = self._end_pos(task), None
        car.claimed = False
        getattr(self, "_done_" + task.kind)(task)

    def _end_pos(self, task):
        lay = self.layout
        if task.spot >= 0:
            return lay.spots[task.spot]
        if task.kind == "charge":
            return lay.charge[task.bay]
        return lay.wash if task.kind == "wash" else lay.curb

    def _park_car(self, task):
        car = task.car
        car.state, car.parked_min = "spot", task.end + SETTLE
        car.pos = self.layout.spots[task.spot]
        self.parked += 1

    def _done_park(self, task):
        task.car.elapsed, task.car.resume = task.car.park_elapsed(task.end), None
        self._park_car(task)

    def _done_relocate(self, task):
        self._free_station(task.car)
        self._park_car(task)

    def _done_charge(self, task):
        car = task.car
        car.elapsed, car.resume = car.park_elapsed(task.end), None
        car.state, car.bay_in, car.pos = "bay", task.end + SETTLE, self.layout.charge[task.bay]

    def _done_wash(self, task):
        car = task.car
        car.elapsed, car.resume = car.park_elapsed(task.end), None
        car.state, car.bay_in, car.pos = "bay", task.end + SETTLE, self.layout.wash

    def _done_deliver(self, task):
        car = task.car
        self.spots[car.spot_index] = None
        wait = task.end + SETTLE - car.req
        unclean = car.dirty and car.service_left > 0
        got = tip(wait, car.impatient, car.expired, car.ev and car.service_left <= 0,
                  car.dirty and car.service_left <= 0, unclean)
        self.delivered += 1
        self.on_time += 0 if car.expired else 1
        self.paid += got > 0
        self.tips += got
        self.cars.remove(car)

    def _abort(self, task, m):
        car = task.car
        self.valet.busy += m - task.start
        self.valet.pos, self.valet.task = car.pos, None
        if task.spot >= 0:
            self.spots[task.spot] = None
        self._free_station(car)

