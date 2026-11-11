using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>The meter's three level colours: gold, ice blue and hot pink (valet_powers.md section 4).</summary>
    public static class PowerColors
    {
        private const float DimFactor = 0.45f;

        public static readonly Color Gold = new Color32(0xFF, 0xC8, 0x3A, 255);
        public static readonly Color IceBlue = new Color32(0x8A, 0xDC, 0xFF, 255);
        public static readonly Color HotPink = new Color32(0xFF, 0x3E, 0xA8, 255);

        /// <summary>The colour of level (or power) 1 to 3; anything else is gold.</summary>
        public static Color ForLevel(int level) => level == 2 ? IceBlue : level == 3 ? HotPink : Gold;

        /// <summary>The level colour darkened, for a notch not reached yet.</summary>
        public static Color Dimmed(int level)
        {
            var c = ForLevel(level);
            return new Color(c.r * DimFactor, c.g * DimFactor, c.b * DimFactor, 1f);
        }
    }
}
