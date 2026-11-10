using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>The archetype picker's answer.</summary>
    public readonly struct PickResult
    {
        public readonly CarArchetype Archetype;
        /// <summary>A dilemma rule boosted the archetype that won the draw.</summary>
        public readonly bool HasRule;
        /// <summary>The strongest rule that boosted the winner (meaningful when <see cref="HasRule"/>).</summary>
        public readonly DilemmaKind Rule;
        /// <summary>That rule's weight multiplier as applied (after assist); 1 without a rule.</summary>
        public readonly float Multiplier;
        /// <summary>A solvable lot-tight rule wants a parked patron's return brought forward, whatever won.</summary>
        public readonly bool WantsEarlyReturn;

        public PickResult(CarArchetype archetype, bool hasRule, DilemmaKind rule, float multiplier, bool wantsEarlyReturn)
        {
            Archetype = archetype; HasRule = hasRule; Rule = rule; Multiplier = multiplier;
            WantsEarlyReturn = wantsEarlyReturn;
        }
    }
}
