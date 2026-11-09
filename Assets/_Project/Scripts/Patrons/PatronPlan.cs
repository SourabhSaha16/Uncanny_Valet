using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>The shift plan as the patrons see it: waiting cap, arrival pace and trait odds. Pure.</summary>
    public sealed class PatronPlan
    {
        public int MaxWaitingCars { get; private set; }
        public float ArrivalGapScale { get; private set; } = 1f;
        public float EvChance { get; private set; }
        public float DirtyChance { get; private set; }
        public float LateEvChance { get; private set; }
        public float LateEvStart { get; private set; }

        /// <summary>Before any plan arrives the config's cap applies and no traits are rolled.</summary>
        public PatronPlan(int defaultMaxWaiting) { MaxWaitingCars = defaultMaxWaiting; }

        public void Apply(ShiftPlan plan)
        {
            MaxWaitingCars = plan.MaxWaitingCars;
            ArrivalGapScale = plan.ArrivalGapScale;
            EvChance = plan.EvChance;
            DirtyChance = plan.DirtyChance;
            LateEvChance = plan.LateEvChance;
            LateEvStart = plan.LateEvStart;
        }

        /// <summary>One roll decides: EV, dirty or plain. A car is never both. The EV chance ramps with shift progress (0..1).</summary>
        public void RollTraits(IPatronRandom random, float shiftProgress, out bool isEv, out bool isDirty)
        {
            var ev = EvChanceCurve.Evaluate(EvChance, LateEvChance, LateEvStart, shiftProgress);
            var r = random.Value();
            isEv = r < ev;
            isDirty = !isEv && r < ev + DirtyChance;
        }
    }
}
