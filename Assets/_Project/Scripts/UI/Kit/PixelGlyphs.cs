namespace UncannyValet.UI.Kit
{
    /// <summary>
    /// The text cell grid and character set of the UI font: a letter body is 5 x 7 virtual pixels (capital height) inside a cell one
    /// blank column wider (<see cref="PixelTextLayout.Advance"/>). Lower case is drawn as upper case; anything unknown is drawn as '?'.
    /// </summary>
    public static class PixelGlyphs
    {
        public const int Width = 5;
        public const int Height = 7;
        public const char Fallback = '?';

        /// <summary>The characters the UI can draw.</summary>
        public const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,:!?-+/$%'#()*";

        /// <summary>How many glyphs the font has.</summary>
        public static int Count => Chars.Length;

        /// <summary>Glyph index of a character, upper-casing letters; unknown characters give the '?' glyph.</summary>
        public static int IndexOf(char c)
        {
            var index = Chars.IndexOf(char.ToUpperInvariant(c));
            return index >= 0 ? index : Chars.IndexOf(Fallback);
        }
    }
}
