using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>What the brain knows about the moment of an archetype draw, besides the lot snapshot.</summary>
    public readonly struct DilemmaContext
    {
        public readonly ArchetypeWeights Weights;
        /// <summary>The pacing band of the arrival (scales the weights).</summary>
        public readonly ScheduleReason Band;
        /// <summary>Relief is on: only plain cars.</summary>
        public readonly bool Relief;
        /// <summary>Workload minus target (form included).</summary>
        public readonly float Gap;
        /// <summary>The target curve is climbing.</summary>
        public readonly bool Rising;
        /// <summary>The shift's arrival budget (the mix bounds are shares of it).</summary>
        public readonly int Budget;

        public DilemmaContext(ArchetypeWeights weights, ScheduleReason band, bool relief, float gap, bool rising, int budget)
        {
            Weights = weights; Band = band; Relief = relief; Gap = gap; Rising = rising; Budget = budget;
        }
    }
}
