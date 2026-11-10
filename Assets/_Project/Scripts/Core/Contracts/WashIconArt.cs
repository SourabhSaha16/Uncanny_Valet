using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// The wash (water drop) icon bitmap shared by the HUD (UI kit) and the wash-slot decal (Lot), so both draw the
    /// same picture. '.' is clear, 'k' ink, 'c' cyan, 'w' white highlight, 'o' dark gold shade. Top row first.
    /// </summary>
    public static class WashIconArt
    {
        public static readonly string[] Rows =
        {
            "....k....", "...kck...", "..kcwck..", "..kcwcck.", ".kcwcccok",
            ".kccccook", ".kcccccok", "..kcccok.", "...kkk...",
        };

        public static readonly Color32 Ink = new Color32(0x1A, 0x16, 0x26, 255);
        public static readonly Color32 Cyan = new Color32(0x38, 0xD6, 0xF0, 255);
        public static readonly Color32 White = new Color32(255, 255, 255, 255);
        public static readonly Color32 Shade = new Color32(0xC9, 0x8A, 0x1E, 255);

        /// <summary>The colour of a bitmap key; false for the clear key '.'.</summary>
        public static bool TryColor(char key, out Color32 color)
        {
            switch (key)
            {
                case 'k': color = Ink; return true;
                case 'c': color = Cyan; return true;
                case 'w': color = White; return true;
                case 'o': color = Shade; return true;
                default: color = default; return false;
            }
        }
    }
}
