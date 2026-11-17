using System;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure text and colour rules of the "+X" power pop.</summary>
    public static class PowerFillPopRule
    {
        /// <summary>"+X" with the amount rounded to a whole number; false when it rounds to zero or less (nothing to show).</summary>
        public static bool TryFormat(float amount, out string text)
        {
            var whole = (int)Math.Round(amount, MidpointRounding.AwayFromZero);
            text = whole > 0 ? "+" + whole : null;
            return whole > 0;
        }

        /// <summary>The pop colour of a source: its own tint pulled toward gold by <paramref name="goldPull"/>, so the set stays subtle; a Perfect Park is the shared bay-border green, unpulled.</summary>
        public static Color ColorOf(PowerFillSource source, float goldPull, Color perfectColor)
        {
            if (source == PowerFillSource.PerfectPark) return perfectColor;
            return Color.Lerp(TintOf(source), PowerColors.Gold, goldPull);
        }

        private static Color TintOf(PowerFillSource source)
        {
            switch (source)
            {
                case PowerFillSource.ParkedInside: return PowerColors.StormGreen;
                case PowerFillSource.PerfectDropOff: return PowerColors.Silver;
                case PowerFillSource.Drift: return PowerColors.IceBlue;
                default: return PowerColors.Gold;
            }
        }
    }
}
