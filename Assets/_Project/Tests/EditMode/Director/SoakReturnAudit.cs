using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>Checks one finished soak shift against the return rules (spec section 5, issue 73).</summary>
    internal static class SoakReturnAudit
    {
        private const float Epsilon = 0.01f;

        /// <summary>Counts pickup-spacing violations: two requests closer than the spacing (less one sim tick, the time resolution), the later one outside a wave peak.</summary>
        public static void CountSpacing(SoakSim sim, SoakProfile profile, float shiftSeconds, float spacing, float tick, SoakResult r)
        {
            for (var i = 1; i < sim.RequestTimes.Count; i++)
            {
                if (sim.RequestTimes[i] - sim.RequestTimes[i - 1] >= spacing - tick - Epsilon) continue;
                var phase = profile.PhaseAt(sim.RequestTimes[i] / shiftSeconds);
                if (phase == DirectorPhase.Peak || phase == DirectorPhase.HarderPeak) continue;
                r.SpacingViolationsAll++;
                if (!sim.RequestFromStation[i] && !sim.RequestFromStation[i - 1]) r.SpacingViolations++;
            }
        }

        /// <summary>Counts cars still waiting at the end (any place). Arrived within the last pickup lead plus the minimum lead and two spacings of the end there is hardly room left for a pickup, so those do not count as missed.</summary>
        public static void CountUnrequested(SoakSim sim, float shiftSeconds, float lastLead, float lead, SoakResult r)
        {
            r.PickupsRequested = sim.RequestTimes.Count;
            foreach (var car in sim.Cars)
            {
                if (car.State == CarLotState.PickupRequested || car.State == CarLotState.Arriving) continue;
                if (car.ParkedAt > shiftSeconds - lastLead - lead) { r.ParkedTooLate++; continue; }
                if (car.State != CarLotState.InStation) r.UnrequestedInSpot++;
                else r.UnrequestedInStation++;
            }
        }

        /// <summary>True when the car's return was moved further than the window allows.</summary>
        public static bool OutsideWindow(SoakCar car, float windowFraction)
            => System.Math.Abs(car.Shifted) > car.PlannedStay * windowFraction + Epsilon;
    }
}
