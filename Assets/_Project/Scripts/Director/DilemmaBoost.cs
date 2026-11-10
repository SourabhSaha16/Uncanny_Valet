using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>What a dilemma rule asks of the archetype draw: more weight for one archetype.</summary>
    public readonly struct DilemmaBoost
    {
        /// <summary>The archetype whose weight is multiplied.</summary>
        public readonly CarArchetype Archetype;
        /// <summary>The weight multiplier (1 or more).</summary>
        public readonly float Multiplier;
        /// <summary>The player can get out of it: a free regular spot, or a pickup due soon that will free one.</summary>
        public readonly bool Solvable;
        /// <summary>The rule also wants a parked patron's return brought forward (lot tight).</summary>
        public readonly bool BringReturnForward;

        public DilemmaBoost(CarArchetype archetype, float multiplier, bool solvable, bool bringReturnForward = false)
        {
            Archetype = archetype; Multiplier = multiplier; Solvable = solvable; BringReturnForward = bringReturnForward;
        }
    }
}
