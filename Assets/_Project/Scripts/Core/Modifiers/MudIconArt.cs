using UnityEngine;

namespace UncannyValet.Core.Modifiers
{
    /// <summary>
    /// The mud splat icon bitmap of the Muddy modifier (the HUD draws it beside the plate). '.' is clear, 'k' ink,
    /// 'm' mud brown, 'b' dark mud, 'h' light mud highlight. Top row first.
    /// </summary>
    public static class MudIconArt
    {
        public static readonly string[] Rows =
        {
            "..k...k..", ".kmk.kmk.", ".kmmkmmk.", "kkmhmmmkk", "kmmhmbmmk",
            "kmmmmmbmk", ".kmmbmmk.", "..kkmkk..", "....k....",
        };

        public static readonly Color32 Ink = new Color32(0x1A, 0x16, 0x26, 255);
        public static readonly Color32 Mud = new Color32(0x7A, 0x4E, 0x2D, 255);
        public static readonly Color32 Dark = new Color32(0x4E, 0x31, 0x1B, 255);
        public static readonly Color32 Light = new Color32(0xA8, 0x74, 0x48, 255);

        /// <summary>The colour of a bitmap key; false for the clear key '.'.</summary>
        public static bool TryColor(char key, out Color32 color)
        {
            switch (key)
            {
                case 'k': color = Ink; return true;
                case 'm': color = Mud; return true;
                case 'b': color = Dark; return true;
                case 'h': color = Light; return true;
                default: color = default; return false;
            }
        }
    }
}
