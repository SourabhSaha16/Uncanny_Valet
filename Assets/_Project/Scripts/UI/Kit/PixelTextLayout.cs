using System.Text;

namespace UncannyValet.UI.Kit
{
    /// <summary>Pure text measuring for the pixel font: one blank column between glyphs.</summary>
    public static class PixelTextLayout
    {
        public const int Advance = PixelGlyphs.Width + 1;
        public const int LineGap = 2;

        /// <summary>Pixel width of a run of glyphs at an integer scale.</summary>
        public static int Width(int length, int scale) => length <= 0 ? 0 : (length * Advance - 1) * scale;

        /// <summary>Pixel height of one line of glyphs at an integer scale.</summary>
        public static int Height(int scale) => PixelGlyphs.Height * scale;

        /// <summary>Distance from one line to the next at an integer scale.</summary>
        public static int LineHeight(int scale) => (PixelGlyphs.Height + LineGap) * scale;

        /// <summary>Left edge that centres content inside a box.</summary>
        public static int Centered(int left, int boxWidth, int contentWidth) => left + (boxWidth - contentWidth) / 2;

        /// <summary>Length of the longest line (lines end at a newline) in a builder.</summary>
        public static int LongestLine(StringBuilder text)
        {
            var longest = 0;
            var current = 0;
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') current = 0;
                else if (++current > longest) longest = current;
            }
            return longest;
        }
    }
}
