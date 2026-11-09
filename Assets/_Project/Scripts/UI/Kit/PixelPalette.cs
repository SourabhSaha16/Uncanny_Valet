using UnityEngine;

namespace UncannyValet.UI.Kit
{
    /// <summary>
    /// The UI colours. Alarm colours follow the style guide: amber = warning, orange-red = danger,
    /// green = good or done, cyan = info and pickups.
    /// </summary>
    public static class PixelPalette
    {
        public static readonly Color Ink = new Color32(0x1A, 0x16, 0x26, 255);
        public static readonly Color Cream = new Color32(0xF4, 0xF0, 0xE6, 255);
        public static readonly Color White = Color.white;
        public static readonly Color Gold = new Color32(0xFF, 0xC8, 0x3A, 255);
        public static readonly Color Amber = new Color32(0xFF, 0xB0, 0x20, 255);
        public static readonly Color OrangeRed = new Color32(0xFF, 0x5A, 0x2A, 255);
        public static readonly Color Green = new Color32(0x4C, 0xCB, 0x5E, 255);
        public static readonly Color Cyan = new Color32(0x38, 0xD6, 0xF0, 255);
        public static readonly Color Grey = new Color32(0x8C, 0x8C, 0x9C, 255);
        public static readonly Color Track = new Color32(0x12, 0x0F, 0x1C, 255);
        public static readonly Color PanelDark = new Color32(0x3A, 0x30, 0x58, 255);
        public static readonly Color PanelCyan = new Color32(0x1E, 0x8A, 0xA0, 255);
        public static readonly Color PanelCyanBright = new Color32(0x2A, 0xB6, 0xCC, 255);
        public static readonly Color PanelCombo = new Color32(0xD0, 0x5A, 0x1E, 255);
        public static readonly Color PanelCream = new Color32(0xE8, 0xDC, 0xB8, 255);
        public static readonly Color Dim = new Color(0f, 0f, 0f, 0.55f);

        private static readonly Color[] KeyTags =
        {
            new Color32(0xE5, 0x48, 0x3C, 255), new Color32(0xF0, 0x8A, 0x2C, 255),
            new Color32(0xF2, 0xD0, 0x30, 255), new Color32(0x4C, 0xCB, 0x5E, 255),
            new Color32(0x28, 0xB8, 0xA8, 255), new Color32(0x3F, 0x7F, 0xE0, 255),
            new Color32(0x8E, 0x5C, 0xD0, 255), new Color32(0xE8, 0x6A, 0xB0, 255),
        };

        /// <summary>The key tag colour for a spot; spot 0 (not parked) is grey.</summary>
        public static Color KeyTag(int spot)
            => spot <= 0 ? Grey : KeyTags[(spot - 1) % KeyTags.Length];
    }
}
