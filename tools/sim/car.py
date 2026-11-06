"""Per-car state during a night simulation."""
from dataclasses import dataclass


@dataclass(eq=False)
class Car:
    id: int
    arrive: float
    stay: float
    impatient: bool
    ev: bool
    dirty: bool
    slot: int
    pos: tuple
    state: str = "queue"      # queue | bay | spot | pickup
    claimed: bool = False     # a valet task is in progress for this car
    resume: float = 0.0       # park-timer running since this minute (None = paused)
    elapsed: float = 0.0      # park-timer minutes used before `resume`
    pause_at: float = None    # minute the valet gets in (timer pauses then)
    bay_in: float = 0.0       # minute the car settled in a station
    service_left: int = 0     # charge / wash minutes still needed
    ready: float = 0.0        # minute the car became ready for its next job (FIFO key)
    parked_min: float = 0.0
    req: float = 0.0          # pickup request minute
    expired: bool = False     # pickup window ran out
    station: object = None    # ("c", bay index) or ("w", 0) while in a station
    spot_index: int = -1

    def park_elapsed(self, m):
        """Park-timer minutes used at minute m (the timer pauses once the valet is driving it)."""
        if self.resume is None:
            return self.elapsed
        end = m if (self.pause_at is None or m < self.pause_at) else self.pause_at
        return self.elapsed + max(0.0, end - self.resume)

    def holds_slot(self, m):
        """Physically in its queue slot at minute m."""
        return self.state == "queue" and not (self.claimed and self.pause_at is not None
                                              and self.pause_at <= m)
