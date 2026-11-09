using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure easing used by the mud wipe: frame-rate independent smoothing toward a 0..1 target.</summary>
    public static class CarTimerFillMath
    {
        /// <summary>Below this gap the fill snaps to its target so it settles exactly.</summary>
        public const float SnapEpsilon = 0.001f;

        /// <summary>Eases <paramref name="current"/> toward the clamped target; frame-rate independent.</summary>
        public static float Smooth(float current, float target, float ratePerSecond, float dt)
        {
            target = Mathf.Clamp01(target);
            current = Mathf.Clamp01(current);
            float blend = 1f - Mathf.Exp(-Mathf.Max(0f, ratePerSecond) * Mathf.Max(0f, dt));
            float next = current + (target - current) * blend;
            return Mathf.Abs(target - next) < SnapEpsilon ? target : next;
        }
    }
}
