using System;

namespace UncannyValet.UI.Screens
{
    /// <summary>Pure: marks which characters of a text belong to one of the highlight phrases (case-sensitive).</summary>
    public static class HighlightMask
    {
        /// <summary>One flag per character of <paramref name="text"/>; true inside any phrase occurrence.</summary>
        public static bool[] Build(string text, string[] phrases)
        {
            var mask = new bool[text.Length];
            for (var p = 0; p < phrases.Length; p++)
            {
                var at = text.IndexOf(phrases[p], StringComparison.Ordinal);
                while (at >= 0)
                {
                    for (var i = 0; i < phrases[p].Length; i++) mask[at + i] = true;
                    at = text.IndexOf(phrases[p], at + phrases[p].Length, StringComparison.Ordinal);
                }
            }
            return mask;
        }
    }
}
