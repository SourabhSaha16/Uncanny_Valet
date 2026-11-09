using UnityEngine;

namespace UncannyValet.Core.Config
{
    /// <summary>
    /// The key-tag colours a ticket can carry. Cars, the floating info box and the key board all draw the same
    /// colour for the same tag index. The values are constants; nothing here changes at runtime.
    /// </summary>
    public static class TagPalette
    {
        /// <summary>Colour shown for a tag index that is not known (yet).</summary>
        public static readonly Color Unknown = new Color32(0x8C, 0x8C, 0x9C, 255);

        private static readonly Color[] Colors =
        {
            new Color32(0xE5, 0x48, 0x3C, 255), new Color32(0xF0, 0x8A, 0x2C, 255),
            new Color32(0xF2, 0xD0, 0x30, 255), new Color32(0x4C, 0xCB, 0x5E, 255),
            new Color32(0x28, 0xB8, 0xA8, 255), new Color32(0x3F, 0x7F, 0xE0, 255),
            new Color32(0x8E, 0x5C, 0xD0, 255), new Color32(0xE8, 0x6A, 0xB0, 255),
            new Color32(0xA6, 0xD6, 0x3C, 255), new Color32(0x66, 0xD0, 0xF5, 255),
            new Color32(0xC8, 0x9A, 0x5A, 255), new Color32(0xF4, 0xF0, 0xE6, 255),
        };

        /// <summary>How many distinct tag colours exist.</summary>
        public static int Count => Colors.Length;

        /// <summary>The colour of a tag index; a negative index gives <see cref="Unknown"/>, a large one wraps.</summary>
        public static Color Get(int tag) => tag < 0 ? Unknown : Colors[tag % Colors.Length];
    }
}
