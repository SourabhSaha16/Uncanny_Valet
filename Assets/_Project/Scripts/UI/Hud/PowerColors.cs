using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>Colours of the power bar: gold bolts and fill, ice blue for freeze time.</summary>
    public static class PowerColors
    {
        private const float DimFactor = 0.45f;

        public static readonly Color Gold = new Color32(0xFF, 0xC8, 0x3A, 255);
        public static readonly Color IceBlue = new Color32(0x8A, 0xDC, 0xFF, 255);

        /// <summary>A colour darkened, for a bolt that is not open yet.</summary>
        public static Color Dimmed(Color color) => new Color(color.r * DimFactor, color.g * DimFactor, color.b * DimFactor, 1f);
    }
}
