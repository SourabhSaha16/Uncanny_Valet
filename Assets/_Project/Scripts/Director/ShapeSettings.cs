using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>
    /// The live numbers that reshape the lot's target curve and car-mix weights (the difficulty dials move them).
    /// Neutral values (scale 1, ramp 1, boost 0) leave the lot profile untouched. Refreshed in place once per tick.
    /// </summary>
    public sealed class ShapeSettings
    {
        private const float NeutralScale = 1f;
        private const float NeutralBoost = 0f;
        private const float DefaultLateStart = 0.6f;

        /// <summary>Multiplier on the target curve.</summary>
        public float TargetScale { get; private set; } = NeutralScale;

        /// <summary>0 flattens the curve to its average; 1 keeps it; above 1 exaggerates it.</summary>
        public float RampShape { get; private set; } = NeutralScale;

        /// <summary>Extra target (fraction) at the very end of the shift.</summary>
        public float LateBoost { get; private set; } = NeutralBoost;

        /// <summary>Fraction of the shift where the late boost starts to ramp in.</summary>
        public float LateStart { get; private set; } = DefaultLateStart;

        /// <summary>True once the four archetype weights have been seeded; otherwise the lot profile's weights apply.</summary>
        public bool HasWeights { get; private set; }

        public ArchetypeWeights Weights { get; private set; }

        /// <summary>True when nothing reshapes the profile.</summary>
        public bool IsNeutral => TargetScale == NeutralScale && RampShape == NeutralScale && LateBoost == NeutralBoost;

        /// <summary>Seeds the neutral shape and the lot's base weights. Bootstrap only.</summary>
        public static void Seed(ITuning t, ArchetypeWeights weights)
        {
            t.Seed(TuningKey.DirTargetScale, NeutralScale);
            t.Seed(TuningKey.DirRampShape, NeutralScale);
            t.Seed(TuningKey.DirLateTargetBoost, NeutralBoost);
            t.Seed(TuningKey.DirWeightPlain, weights.Plain);
            t.Seed(TuningKey.DirWeightDirty, weights.Dirty);
            t.Seed(TuningKey.DirWeightEv, weights.Ev);
            t.Seed(TuningKey.DirWeightDirtyEv, weights.DirtyEv);
        }

        public void Read(ITuning t)
        {
            TargetScale = TuningRead.Float(t, TuningKey.DirTargetScale, TargetScale);
            RampShape = TuningRead.Float(t, TuningKey.DirRampShape, RampShape);
            LateBoost = TuningRead.Float(t, TuningKey.DirLateTargetBoost, LateBoost);
            LateStart = TuningRead.Float(t, TuningKey.PlanLateEvStart, LateStart);
            HasWeights = t.TryGet(TuningKey.DirWeightPlain, out var plain) & t.TryGet(TuningKey.DirWeightDirty, out var dirty)
                & t.TryGet(TuningKey.DirWeightEv, out var ev) & t.TryGet(TuningKey.DirWeightDirtyEv, out var dirtyEv);
            if (HasWeights) Weights = new ArchetypeWeights(plain, dirty, ev, dirtyEv);
        }
    }
}
