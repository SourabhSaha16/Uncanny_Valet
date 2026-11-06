"""Pure game rules ported from the C# code: tips and patron rolls."""
import math
from dataclasses import dataclass
from params import DEFAULT_PLANS, PATRON, TIP


@dataclass(frozen=True)
class Plan:
    """One night's rules (Core/Events/NightPlan.cs). gap_scale < 1 is busier."""
    night: int
    cap: int
    gap_scale: float
    ev: float
    dirty: float
    charging: bool
    wash: bool

    def with_(self, **kw):
        return Plan(**{**self.__dict__, **kw})


def default_plan(night):
    return Plan(*DEFAULT_PLANS[min(max(night, 1), len(DEFAULT_PLANS)) - 1])


def round_half_away(x):
    return math.floor(x + 0.5)


def tip(wait, impatient, expired, charged_ev, cleaned, unclean):
    """Economy/TipCalculator.cs: base +- speed, charge and clean bonuses, never below 0."""
    if expired or unclean:
        return 0
    if wait <= TIP["window"]:
        speed = TIP["impatient_bonus"] if impatient else TIP["bonus"]
    else:
        speed = -round_half_away((wait - TIP["window"]) * TIP["penalty_per_min"])
    total = PATRON["base_tip"] + speed
    total += TIP["charge_bonus"] if charged_ev else 0
    total += TIP["clean_bonus"] if cleaned else 0
    return max(0, total)


def roll_car(rng, plan):
    """Patrons/PatronRequestRoller.Roll: stay, impatient, one trait roll (EV, dirty or plain)."""
    stay = rng.uniform(*PATRON["stay"])
    impatient = rng.random() < PATRON["impatient"]
    r = rng.random()
    ev = r < plan.ev
    dirty = (not ev) and r < plan.ev + plan.dirty
    return stay, impatient, ev, dirty


def next_gap(rng, plan):
    """Patrons/PatronRequestRoller.NextGap."""
    return rng.uniform(*PATRON["gap"]) * plan.gap_scale
