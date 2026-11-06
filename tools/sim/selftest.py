"""Sanity checks for the simulation. Run: python tools/sim/selftest.py (prints OK or fails with an assert)."""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from geometry import build_layout                  # noqa: E402
from night import run_night                        # noqa: E402
from policies import POLICIES, leg_seconds         # noqa: E402
from rules import default_plan, tip                # noqa: E402


def test_tips():
    """Mirrors Tests/EditMode/Economy/TipCalculatorTests: base 20, window 10, +2 / +6, -0.5 per minute."""
    assert tip(5, False, False, False, False, False) == 22
    assert tip(5, True, False, False, False, False) == 26
    assert tip(30, False, False, False, False, False) == 10
    assert tip(80, False, False, False, False, False) == 0          # never below 0
    assert tip(5, False, True, False, False, False) == 0            # pickup expired
    assert tip(5, False, False, False, False, True) == 0            # dirty and unwashed
    assert tip(5, False, False, True, True, False) == 52            # charge and clean bonuses


def test_geometry():
    """12 spots in 2 rows reproduce LotConfig: first spot centre (-6.2, 4.7), curb at (-12, 0)."""
    lay = build_layout(12, 3)
    assert abs(lay.spots[0][0] + 6.2) < 1e-6 and abs(lay.spots[0][1] - 4.7) < 1e-6
    assert abs(lay.queue[0][1] + 6.05) < 1e-6 and abs(lay.queue[1][0] + 14.8) < 1e-6
    assert len(lay.charge) == 2 and abs(lay.wash[0] + 11.5) < 1e-6


def test_motion():
    assert abs(leg_seconds(10, 8, 6, 12) - (8 / 6 + 8 / 12 + (10 - 8 * 8 / 12 - 8 * 8 / 24) / 8)) < 1e-9
    assert leg_seconds(0, 8, 6, 12) == 0


def test_invariants():
    """Seeded runs are reproducible and obey basic bookkeeping."""
    for night in (1, 4, 7):
        for pol in POLICIES.values():
            a = run_night(default_plan(night), 6, pol, "selftest-1")
            assert a == run_night(default_plan(night), 6, pol, "selftest-1")
            arrivals, parked, timeouts, pickups, delivered, on_time, tips, util, peak, full, spots = a
            assert parked + timeouts <= arrivals and delivered <= pickups <= parked
            assert on_time <= delivered and tips >= 0 and 0 <= util <= 1.0001
            assert spots <= 6 and peak <= arrivals


if __name__ == "__main__":
    for t in (test_tips, test_geometry, test_motion, test_invariants):
        t()
    print("OK: 4 groups of checks passed")
