using System.Collections.Generic;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure data: a tiny 5 by 7 pixel font, just the letters the road labels need (ENTRY, EXIT). Compose turns a word
    /// into one bitmap for <see cref="PixelDecal"/>: '#' is ink, '.' is clear.
    /// </summary>
    public static class PixelFont
    {
        /// <summary>Pixels in a letter's height.</summary>
        public const int Height = 7;
        /// <summary>Clear columns between two letters.</summary>
        public const int Spacing = 1;
        /// <summary>The key that marks an inked pixel in a bitmap.</summary>
        public const char Ink = '#';

        private const char Clear = '.';

        private static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string[]>
        {
            ['E'] = new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#####" },
            ['N'] = new[] { "#...#", "##..#", "##..#", "#.#.#", "#..##", "#..##", "#...#" },
            ['T'] = new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.." },
            ['R'] = new[] { "####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#" },
            ['Y'] = new[] { "#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.." },
            ['X'] = new[] { "#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#" },
            ['I'] = new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "#####" },
            ['O'] = new[] { ".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." },
            ['P'] = new[] { "####.", "#...#", "#...#", "####.", "#....", "#....", "#...." },
        };

        /// <summary>True when the font has a glyph for <paramref name="letter"/>.</summary>
        public static bool Has(char letter) => Glyphs.ContainsKey(letter);

        /// <summary>The word as <see cref="Height"/> rows of equal width; a letter the font lacks is skipped.</summary>
        public static string[] Compose(string word)
        {
            var rows = new string[Height];
            for (var row = 0; row < Height; row++)
            {
                var line = new System.Text.StringBuilder();
                foreach (var letter in word)
                {
                    if (!Glyphs.TryGetValue(letter, out var glyph)) continue;
                    if (line.Length > 0) line.Append(Clear, Spacing);
                    line.Append(glyph[row]);
                }
                rows[row] = line.ToString();
            }
            return rows;
        }
    }
}
