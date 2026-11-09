using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure maths for the pickup flash: how fast, which colour level, and the on/off phase.</summary>
    public static class PickupFlashMath
    {
        /// <summary>Flashes per second for a pickup whose patience is <paramref name="elapsed01"/> used up (0 calm, 1 gone).</summary>
        public static float Frequency(float elapsed01, float minHz, float maxHz)
            => Mathf.Lerp(minHz, maxHz, Mathf.Clamp01(elapsed01));

        /// <summary>Moves the flash phase (0..1, wrapping) forward by <paramref name="hz"/> for <paramref name="dt"/> seconds.</summary>
        public static float Advance(float phase, float hz, float dt)
        {
            var next = phase + Mathf.Max(0f, hz) * Mathf.Max(0f, dt);
            return next - Mathf.Floor(next);
        }

        /// <summary>True for the first half of every phase cycle.</summary>
        public static bool IsOn(float phase) => phase < 0.5f;

        /// <summary>Calm until <paramref name="amberFrom"/>, amber until <paramref name="dangerFrom"/>, then danger. Expired is always danger.</summary>
        public static FlashLevel Level(float elapsed01, bool expired, float amberFrom, float dangerFrom)
        {
            if (expired || elapsed01 >= dangerFrom) return FlashLevel.Danger;
            return elapsed01 >= amberFrom ? FlashLevel.Amber : FlashLevel.Calm;
        }
    }
}
