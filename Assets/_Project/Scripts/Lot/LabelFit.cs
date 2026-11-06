using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Pure maths: the largest label size that keeps a line of text inside a given width.</summary>
    public static class LabelFit
    {
        /// <summary>
        /// Returns <paramref name="maxSize"/>, or smaller if the text would not fit.
        /// <paramref name="charWidthPerSize"/> is the width of one character per unit of size (generous);
        /// <paramref name="fillFraction"/> is the share of the width the text may use (the rest is margin).
        /// </summary>
        public static float Size(int characters, float width, float maxSize, float charWidthPerSize, float fillFraction)
        {
            if (characters <= 0 || charWidthPerSize <= 0f) return maxSize;
            var fit = width * Mathf.Clamp01(fillFraction) / (characters * charWidthPerSize);
            return Mathf.Min(maxSize, fit);
        }
    }
}
