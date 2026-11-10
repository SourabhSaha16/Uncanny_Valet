using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>Everything the archetype picker reads for one draw.</summary>
    public readonly struct PickRequest
    {
        public readonly LotSnapshot Lot;
        /// <summary>The lot's base weights.</summary>
        public readonly ArchetypeWeights Weights;
        /// <summary>The pacing band (UnderTarget, Idle, OnTarget, OverTarget) that scales the weights.</summary>
        public readonly ScheduleReason Band;
        /// <summary>Relief: only a plain car may be sent.</summary>
        public readonly bool Relief;
        /// <summary>The dilemma budget allows rule boosts for this draw.</summary>
        public readonly bool DilemmaAllowed;
        /// <summary>Cars the shift is meant to bring; the mix bounds are shares of this.</summary>
        public readonly int Budget;

        public PickRequest(in LotSnapshot lot, ArchetypeWeights weights, ScheduleReason band, bool relief,
            bool dilemmaAllowed, int budget)
        {
            Lot = lot; Weights = weights; Band = band; Relief = relief; DilemmaAllowed = dilemmaAllowed; Budget = budget;
        }
    }
}
