using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Which part of a mud puddle a pixel belongs to.</summary>
    public enum MudTone
    {
        /// <summary>Outside the puddle: transparent.</summary>
        None,
        /// <summary>Dark brown inside.</summary>
        Body,
        /// <summary>Lighter brown rim along the edge.</summary>
        Rim,
        /// <summary>A lighter ripple ring inside.</summary>
        Ripple,
    }

    /// <summary>Pure pixel rules of a mud puddle: a wobbly round blob with a lighter rim and a few partial ripple rings. Each variant has its own outline.</summary>
    public static class MudBlobPixels
    {
        /// <summary>Different outlines to choose from.</summary>
        public const int Variants = 3;

        private const float BaseRadius = 0.82f;
        private const float RimWidth = 0.13f;
        private const float RippleWidth = 0.05f;
        private const float RippleInner = 0.38f;
        private const float RippleOuter = 0.62f;

        /// <summary>The part of the puddle at pixel (x, y) of an n by n picture.</summary>
        public static MudTone Sample(int x, int y, int n, int variant)
        {
            var half = n * 0.5f;
            var dx = (x + 0.5f - half) / half;
            var dy = (y + 0.5f - half) / half;
            var distance = Mathf.Sqrt(dx * dx + dy * dy);
            var angle = Mathf.Atan2(dy, dx);
            var radius = Outline(angle, variant);
            if (distance > radius) return MudTone.None;
            if (distance > radius - RimWidth) return MudTone.Rim;
            return IsRipple(distance, angle, radius, variant) ? MudTone.Ripple : MudTone.Body;
        }

        private static float Outline(float angle, int variant)
        {
            var seed = variant * 1.7f;
            return BaseRadius + 0.1f * Mathf.Sin(3f * angle + seed) + 0.06f * Mathf.Sin(5f * angle + 2f * seed);
        }

        // Two rings, each drawn only along part of its circle so they read as ripples, not targets.
        private static bool IsRipple(float distance, float angle, float radius, int variant)
        {
            var seed = variant * 2.3f;
            var inner = Mathf.Abs(distance - RippleInner * radius) < RippleWidth && Mathf.Sin(angle * 2f + seed) > 0.2f;
            var outer = Mathf.Abs(distance - RippleOuter * radius) < RippleWidth && Mathf.Sin(angle * 3f + seed) > 0.5f;
            return inner || outer;
        }
    }
}
