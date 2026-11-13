using System;

namespace UncannyValet.Economy
{
    /// <summary>Maps car damage (0..1) to the share of the blessing it costs, in whole stages. Pure.</summary>
    public static class DamagePenalty
    {
        /// <summary>True when the damage is total (the car is wrecked) and the blessing is 0.</summary>
        public static bool IsWrecked(float damage01) => damage01 >= 1f;

        /// <summary>Share 0..1 of the blessing lost: one penalty step for every full stage of damage, capped at 1.</summary>
        public static float Share(float damage01, float stageSize, float penaltyPerStage)
        {
            if (damage01 <= 0f || stageSize <= 0f) return 0f;
            var stages = (float)Math.Floor(damage01 / stageSize + 1e-4f);
            return Math.Min(1f, stages * penaltyPerStage);
        }
    }
}
