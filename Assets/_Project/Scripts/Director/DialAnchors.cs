using System.Collections.Generic;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>
    /// The designer-chosen anchor table of the difficulty dials (data, not logic; the config asset copies it and can
    /// be edited). Each row reads: dial, derived key, effect at dial 0, effect at dial 10. The effect at the default
    /// dial position is always "no change". Scale anchors multiply the key's starting value.
    /// </summary>
    public static class DialAnchors
    {
        private const TuningKey Intensity = TuningKey.DialIntensity;
        private const TuningKey Ramp = TuningKey.DialRamp;
        private const TuningKey Rush = TuningKey.DialLateRush;
        private const TuningKey Dirty = TuningKey.DialDirtyCars;
        private const TuningKey Ev = TuningKey.DialEvCars;

        /// <summary>Every link of every dial.</summary>
        public static List<DialLink> All()
        {
            var all = new List<DialLink>();
            all.AddRange(Pacing());
            all.AddRange(CarMix());
            all.AddRange(DialPlayerAnchors.All());
            all.AddRange(DialDilemmaAnchors.All());
            return all;
        }

        // Higher intensity = harder: a higher target, cars sooner and closer together, more cars, more waiting.
        private static DialLink[] Pacing() => new[]
        {
            new DialLink(Intensity, TuningKey.DirTargetScale, 0.5f, 1.4f),
            new DialLink(Intensity, TuningKey.DirUnderFactor, 1.3f, 0.65f),
            new DialLink(Intensity, TuningKey.DirOverFactor, 1.4f, 0.8f),
            new DialLink(Intensity, TuningKey.DirMinInterval, 1.5f, 0.6f),
            new DialLink(Intensity, TuningKey.DirMaxInterval, 1.4f, 0.75f),
            new DialLink(Intensity, TuningKey.DirArrivalBudget, 0.65f, 1.4f),
            new DialLink(Intensity, TuningKey.DirBudgetSlack, 0.5f, 2f),
            new DialLink(Intensity, TuningKey.PlanMaxWaitingCars, 0.67f, 1f),
            // Steeper ramp = the curve's rise and dip is kept (0 flat, 1 as authored, 1.6 exaggerated).
            new DialLink(Ramp, TuningKey.DirRampShape, 0f, 1.6f),
            // Stronger rush = more EVs at the end, starting earlier, and a higher target workload at the very end.
            new DialLink(Rush, TuningKey.PlanLateEvChance, 0.4f, 1.6f),
            new DialLink(Rush, TuningKey.PlanLateEvStart, 1.3f, 0.7f),
            new DialLink(Rush, TuningKey.DirLateTargetBoost, -0.3f, 0.4f, DialEffect.Add),
        };

        // More dirty or EV cars: higher chances and weights; dirty EVs follow both dials; dirty streaks may run longer.
        private static DialLink[] CarMix() => new[]
        {
            new DialLink(Dirty, TuningKey.PlanDirtyChance, 0f, 2f),
            new DialLink(Dirty, TuningKey.DirWeightDirty, 0f, 2.5f),
            new DialLink(Dirty, TuningKey.DirWeightDirtyEv, 0f, 2.5f),
            new DialLink(Dirty, TuningKey.DirStreakLimit, 0.5f, 1.5f),
            new DialLink(Ev, TuningKey.PlanEvChance, 0f, 2f),
            new DialLink(Ev, TuningKey.PlanLateEvChance, 0f, 2f),
            new DialLink(Ev, TuningKey.DirWeightEv, 0f, 2.5f),
            new DialLink(Ev, TuningKey.DirWeightDirtyEv, 0f, 2.5f),
        };
    }
}
