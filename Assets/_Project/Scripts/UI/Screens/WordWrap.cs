using System.Collections.Generic;

namespace UncannyValet.UI.Screens
{
    /// <summary>Pure word wrapping: splits text into spans of at most a given number of characters, breaking at spaces.</summary>
    public static class WordWrap
    {
        /// <summary>One wrapped line, as a range of the source text.</summary>
        public readonly struct Span
        {
            public readonly int Start;
            public readonly int Length;

            public Span(int start, int length)
            {
                Start = start;
                Length = length;
            }
        }

        /// <summary>Appends the wrapped lines of <paramref name="text"/> to <paramref name="spans"/>. A word longer than a line is split.</summary>
        public static void Wrap(string text, int maxChars, List<Span> spans)
        {
            var start = 0;
            while (start < text.Length)
            {
                var end = LineEnd(text, start, maxChars);
                spans.Add(new Span(start, end - start));
                start = end;
                while (start < text.Length && text[start] == ' ') start++;
            }
        }

        private static int LineEnd(string text, int start, int maxChars)
        {
            if (text.Length - start <= maxChars) return text.Length;
            var cut = text.LastIndexOf(' ', start + maxChars, maxChars + 1);
            return cut > start ? cut : start + maxChars;
        }
    }
}
