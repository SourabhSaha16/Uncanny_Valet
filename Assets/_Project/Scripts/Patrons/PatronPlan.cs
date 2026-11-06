using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>The night plan as the patrons see it: waiting cap, arrival pace and trait odds. Pure.</summary>
    public sealed class PatronPlan
    {
        public int MaxWaitingCars { get; private set; }
        public float ArrivalGapScale { get; private set; } = 1f;
        public float EvChance { get; private set; }
        public float DirtyChance { get; private set; }

        /// <summary>Before any plan arrives the config's cap applies and no traits are rolled.</summary>
        public PatronPlan(int defaultMaxWaiting) { MaxWaitingCars = defaultMaxWaiting; }

        public void Apply(NightPlan plan)
        {
            MaxWaitingCars = plan.MaxWaitingCars;
            ArrivalGapScale = plan.ArrivalGapScale;
            EvChance = plan.EvChance;
            DirtyChance = plan.DirtyChance;
        }

        /// <summary>One roll decides: EV, dirty or plain. A car is never both.</summary>
        public void RollTraits(IPatronRandom random, out bool isEv, out bool isDirty)
        {
            var r = random.Value();
            isEv = r < EvChance;
            isDirty = !isEv && r < EvChance + DirtyChance;
        }
    }
}
