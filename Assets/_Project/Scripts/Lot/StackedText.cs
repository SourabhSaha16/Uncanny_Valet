using System.Text;

namespace UncannyValet.Lot
{
    /// <summary>Pure text helper: writes a word vertically, one letter per line.</summary>
    public static class StackedText
    {
        /// <summary>The word with a line break between its letters ("CURB" becomes C, U, R, B on four lines).</summary>
        public static string Stack(string word)
        {
            if (string.IsNullOrEmpty(word)) return string.Empty;
            var text = new StringBuilder(word.Length * 2);
            for (var i = 0; i < word.Length; i++)
            {
                if (i > 0) text.Append('\n');
                text.Append(word[i]);
            }
            return text.ToString();
        }

        /// <summary>X offset of column <paramref name="index"/> of <paramref name="count"/> columns, centred on zero.</summary>
        public static float ColumnOffset(int index, int count, float spacing)
            => (index - (count - 1) * 0.5f) * spacing;
    }
}
