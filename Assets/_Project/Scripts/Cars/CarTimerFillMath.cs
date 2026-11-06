using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure maths for the timer overlay: smoothing and the bottom-up fill rectangle.</summary>
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

        /// <summary>Local Y scale of the overlay (body height is 1 in the car's local space).</summary>
        public static float HeightScale(float fill01) => Mathf.Clamp01(fill01);

        /// <summary>Local Y centre of an overlay anchored to the bottom edge of a unit-height body.</summary>
        public static float CentreY(float fill01) => -0.5f + Mathf.Clamp01(fill01) * 0.5f;
    }
}
