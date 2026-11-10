using System;
using UncannyValet.UI.Kit;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure sizes of a licence plate box: a light plate with a dark border and pixel text.
    /// The word gap is narrower than a full glyph so "KT 4" stays small enough to sit under a car.
    /// </summary>
    public static class PlateLayout
    {
        public const int Border = 1;
        public const int PadX = 2;
        public const int PadY = 1;
        /// <summary>Columns a space takes between the letters and the digit.</summary>
        public const int SpaceAdvance = 2;
        /// <summary>Thickness of the flashing glow drawn around a plate whose car is wanted back.</summary>
        public const int Glow = 2;

        /// <summary>Pixel width of the plate text (glyphs plus gaps, narrow spaces).</summary>
        public static int TextWidth(string plate)
        {
            if (string.IsNullOrEmpty(plate)) return 0;
            var width = -1;
            for (var i = 0; i < plate.Length; i++)
                width += plate[i] == ' ' ? SpaceAdvance : PixelTextLayout.Advance;
            return width;
        }

        /// <summary>Roomy plate width (text, padding and border), used on the key board.</summary>
        public static int Width(string plate) => TextWidth(plate) + 2 * (Border + PadX);

        /// <summary>Smallest readable plate width: the text and its border, no padding.</summary>
        public static int MinWidth(string plate) => TextWidth(plate) + 2 * Border;

        /// <summary>Plate width under a car that is <paramref name="carWidthPixels"/> wide: the car's width, or the smallest readable box if the car is narrower.</summary>
        public static int WidthFor(string plate, int carWidthPixels) => Math.Max(MinWidth(plate), carWidthPixels);

        /// <summary>Plate height in virtual pixels.</summary>
        public static int Height => PixelGlyphs.Height + 2 * (Border + PadY);

    }
}
