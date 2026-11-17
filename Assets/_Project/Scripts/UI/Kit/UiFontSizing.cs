using System;

namespace UncannyValet.UI.Kit
{
    /// <summary>Pure maths for the UI font: how many screen pixels tall to ask the font for, so capitals fill a text row.</summary>
    public static class UiFontSizing
    {
        /// <summary>
        /// Font size in screen pixels for text at <paramref name="textScale"/> where one virtual pixel is <paramref name="unit"/> screen pixels.
        /// With <paramref name="snap"/> above 1 the size rounds to a multiple of it (never below it); the result never exceeds <paramref name="maxSize"/>.
        /// </summary>
        public static int Size(int textScale, float unit, float emPerRow, int maxSize, int snap)
        {
            var ideal = PixelGlyphs.Height * Math.Max(1, textScale) * unit * emPerRow;
            var size = snap > 1 ? Math.Max(snap, (int)Math.Round(ideal / snap) * snap) : Math.Max(1, (int)Math.Round(ideal));
            return Math.Min(size, maxSize);
        }
    }
}
