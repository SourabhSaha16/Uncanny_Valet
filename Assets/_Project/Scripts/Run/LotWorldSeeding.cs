using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Run
{
    /// <summary>
    /// Seeds what a lot decides about its world into the tuning store: the crowd size, whether ambient road cars exist and whether a late-ramping
    /// service modifier may ever appear. Pure, so the tests and Bootstrap share it.
    /// </summary>
    public static class LotWorldSeeding
    {
        /// <summary>Seeds the pedestrian count, the ambient traffic keys (0 when the lot has none) and the late service ramp (0 when no ramping modifier is offered).</summary>
        public static void Seed(ITuning tuning, LotDefinition lot)
        {
            tuning.Seed(TuningKey.PedestrianCount, lot.AmbientPedestrians);
            if (!lot.AmbientTraffic)
            {
                tuning.Seed(TuningKey.TrafficMinOnScreen, 0f);
                tuning.Seed(TuningKey.TrafficTargetOnScreen, 0f);
            }
            if (LateRampSwitchedOff(lot)) tuning.Seed(TuningKey.PlanLateEvChance, 0f);
        }

        // A lot that lists a late-ramping modifier (needs charge) only with chance 0 offers none of it, so the end-of-shift surge must not bring it back.
        private static bool LateRampSwitchedOff(LotDefinition lot)
        {
            var listed = false;
            for (var i = 0; i < lot.ModifierChances.Count; i++)
            {
                if (!lot.ModifierChances[i].Modifier.RampsLate) continue;
                if (lot.ModifierChances[i].Chance > 0f) return false;
                listed = true;
            }
            return listed;
        }
    }
}
