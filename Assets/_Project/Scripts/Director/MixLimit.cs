using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>
    /// Pure bounded-mix rule (spec section 5): over the shift each archetype's share stays within a tolerance of its
    /// base share. An archetype that would pass its upper bound is removed from the draw; when the cars still to come
    /// are only just enough to lift every archetype to its lower bound, the draw is limited to those that need it.
    /// </summary>
    internal static class MixLimit
    {
        private const float Epsilon = 0.0001f;

        /// <param name="weights">The draw weights; limited archetypes are set to 0.</param>
        /// <param name="baseWeights">The unlocked archetypes' base weights; their shares are the targets.</param>
        /// <param name="budget">Cars the shift is meant to bring.</param>
        /// <param name="tolerance">Allowed distance from the base share (0.1 = 10 points).</param>
        /// <param name="lookahead">Cars of slack before the end: the draw is limited to the needy this early, so the streak limit can still be kept.</param>
        public static void Apply(float[] weights, float[] baseWeights, ArchetypeHistory history, int budget, float tolerance,
            int lookahead)
        {
            var sum = 0f;
            for (var i = 0; i < baseWeights.Length; i++) sum += baseWeights[i];
            if (sum <= 0f || budget <= 0) return;
            var total = history.Picks + 1 > budget ? history.Picks + 1 : budget;
            var needTotal = 0;
            for (var i = 0; i < baseWeights.Length; i++)
            {
                if (baseWeights[i] <= 0f) continue;
                var share = baseWeights[i] / sum;
                if (history.Count((CarArchetype)i) + 1 > (share + tolerance) * total + Epsilon) weights[i] = 0f;
                needTotal += Need(history, (CarArchetype)i, share, tolerance, budget);
            }
            if (needTotal <= 0 || needTotal + lookahead < budget - history.Picks) return;
            for (var i = 0; i < baseWeights.Length; i++)
                if (Need(history, (CarArchetype)i, baseWeights[i] / sum, tolerance, budget) <= 0) weights[i] = 0f;
        }

        // Cars of this archetype still needed to reach its lower bound by the end of the budget.
        private static int Need(ArchetypeHistory history, CarArchetype a, float share, float tolerance, int budget)
        {
            var lower = (int)System.Math.Ceiling((share - tolerance) * budget - Epsilon);
            var need = lower - history.Count(a);
            return need > 0 ? need : 0;
        }
    }
}
