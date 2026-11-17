using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>The shift plan as the patrons see it: the waiting cap, the arrival pace and the late-shift ramp. Pure.</summary>
    public sealed class PatronPlan
    {
        /// <summary>The late chance before any plan arrives: no ramp, the lot's chance applies all shift.</summary>
        public const float NoLateChance = -1f;

        public int MaxWaitingCars { get; private set; }
        public float ArrivalGapScale { get; private set; } = 1f;

        /// <summary>Chance a late-ramping modifier reaches at the end of the shift; <see cref="NoLateChance"/> before a plan arrives.</summary>
        public float LateChance { get; private set; } = NoLateChance;

        /// <summary>Fraction 0..1 of the shift after which the late chance starts ramping.</summary>
        public float LateStart { get; private set; }

        /// <summary>Before any plan arrives the config's cap applies and no ramp is set.</summary>
        public PatronPlan(int defaultMaxWaiting) { MaxWaitingCars = defaultMaxWaiting; }

        public void Apply(ShiftPlan plan)
        {
            MaxWaitingCars = plan.MaxWaitingCars;
            ArrivalGapScale = plan.ArrivalGapScale;
            LateChance = plan.LateEvChance;
            LateStart = plan.LateEvStart;
        }
    }
}
