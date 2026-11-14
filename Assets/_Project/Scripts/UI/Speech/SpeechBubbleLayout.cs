using UncannyValet.UI.Kit;

namespace UncannyValet.UI.Speech
{
    /// <summary>Pure sizes and positions of a speech bubble on the pixel canvas.</summary>
    public static class SpeechBubbleLayout
    {
        /// <summary>Longest text line in characters; a 40 character line wraps onto two.</summary>
        public const int LineChars = 24;
        public const int Padding = 5;
        public const int NameGap = 3;
        public const int TailHeight = 3;

        /// <summary>World units above the speaker's feet where the bubble's tail points.</summary>
        public const float LiftWorldUnits = 1.1f;

        /// <summary>Bubble width for the longest of the name and the text lines (in characters).</summary>
        public static int Width(int nameChars, int longestLine)
            => PixelTextLayout.Width(System.Math.Max(nameChars, longestLine), 1) + Padding * 2;

        /// <summary>Bubble height for a number of text lines.</summary>
        public static int Height(int lines)
            => Padding * 2 + PixelGlyphs.Height + NameGap + lines * PixelTextLayout.LineHeight(1) - PixelTextLayout.LineGap;

        /// <summary>Keeps <paramref name="start"/> so that a box of <paramref name="size"/> stays inside 0..<paramref name="limit"/>.</summary>
        public static int Clamp(int start, int size, int limit)
            => size >= limit ? 0 : System.Math.Max(0, System.Math.Min(start, limit - size));
    }
}
