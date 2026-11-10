using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// The plug icon bitmap shared by the HUD info box (UI kit) and the charging-bay decal (Lot), so both draw the
    /// same picture. '.' is clear, 'k' ink, 'y' gold contacts, 'n' green body. Top row first.
    /// </summary>
    public static class PlugIconArt
    {
        public static readonly string[] Rows =
        {
            "..k...k..", "..y...y..", "..y...y..", ".kkkkkkk.", ".knnnnnk.",
            ".knnnnnk.", "..knnnk..", "...knk...", "...kkk...",
        };

        public static readonly Color32 Ink = new Color32(0x1A, 0x16, 0x26, 255);
        public static readonly Color32 Gold = new Color32(0xFF, 0xC8, 0x3A, 255);
        public static readonly Color32 Green = new Color32(0x4C, 0xCB, 0x5E, 255);

        /// <summary>The colour of a bitmap key; false for the clear key '.'.</summary>
        public static bool TryColor(char key, out Color32 color)
        {
            switch (key)
            {
                case 'k': color = Ink; return true;
                case 'y': color = Gold; return true;
                case 'n': color = Green; return true;
                default: color = default; return false;
            }
        }
    }
}
