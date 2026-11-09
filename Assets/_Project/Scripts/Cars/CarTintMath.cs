using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure colour maths for the per-ticket body tint and the pickup pulse.</summary>
    public static class CarTintMath
    {
        /// <summary>The tag colour lerped toward white by <paramref name="softness"/> (0 keeps it, 1 is white).</summary>
        public static Color Soften(Color tag, float softness)
            => Color.Lerp(tag, Color.white, Mathf.Clamp01(softness));

        /// <summary>
        /// The colour multiplied into a car sprite. Regular cars get the softened tag colour; an EV gets only
        /// <paramref name="evStrength"/> of it, so its own blue look survives.
        /// </summary>
        public static Color BodyTint(Color tag, float softness, bool isEv, float evStrength)
        {
            var soft = Soften(tag, softness);
            return isEv ? Color.Lerp(Color.white, soft, Mathf.Clamp01(evStrength)) : soft;
        }

        /// <summary>The tint while a pickup pulse is on (moved toward the flash colour) or off (unchanged).</summary>
        public static Color Pulse(Color tint, Color flash, float mix, bool on)
            => on ? Color.Lerp(tint, flash, Mathf.Clamp01(mix)) : tint;
    }
}
