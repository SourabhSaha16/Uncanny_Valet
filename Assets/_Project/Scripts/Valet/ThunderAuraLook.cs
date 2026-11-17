using System;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>The tunable look of the Thunder stride glow: colours, glow size and how often it throws a spark of lightning.</summary>
    [Serializable]
    public struct ThunderAuraLook
    {
        [Tooltip("Colour the valet or his car is multiplied by at full strength.")]
        public Color Tint;
        [Tooltip("Colour of the soft glow under the valet or his car (its alpha is the glow's full opacity).")]
        public Color Glow;
        [Min(0.5f), Tooltip("Glow width as a multiple of the width of the valet or car.")]
        public float GlowScale;
        [Min(0.02f), Tooltip("Seconds between the sparks of lightning thrown around the valet or car, even when standing still.")]
        public float SparkInterval;
        [Min(0f), Tooltip("How far around the target a spark can start and end, in world units.")]
        public float SparkReach;

        /// <summary>The shipped look: golden tint, warm yellow glow, a spark about every 0.12 seconds.</summary>
        public static ThunderAuraLook Default => new ThunderAuraLook
        {
            Tint = new Color(1f, 0.85f, 0.3f, 1f),
            Glow = new Color(1f, 0.9f, 0.3f, 0.7f),
            GlowScale = 1.8f,
            SparkInterval = 0.12f,
            SparkReach = 0.6f,
        };
    }
}
