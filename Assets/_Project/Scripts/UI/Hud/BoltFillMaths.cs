using System;
using UncannyValet.Core.Contracts;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure maths of the three-bolt power meter: each bolt's own fill share, its lit rows and the pulse of a full bolt.</summary>
    public static class BoltFillMaths
    {
        /// <summary>
        /// How full bolt <paramref name="bolt"/> (1 to 3) is, 0 to 1, when the whole bar is at <paramref name="fill01"/>.
        /// A bolt spans the bar between the previous threshold and its own, so bolt 1 fills first.
        /// </summary>
        public static float Share(float fill01, int bolt)
        {
            var from = bolt <= 1 ? 0f : PowerBarThresholds.FractionOf(bolt - 1);
            var to = PowerBarThresholds.FractionOf(bolt);
            return Math.Min(Math.Max((fill01 - from) / (to - from), 0f), 1f);
        }

        /// <summary>Rows lit from the bottom of a bolt that is <paramref name="share"/> full and <paramref name="rows"/> tall; a bolt above zero lights at least one row.</summary>
        public static int LitRows(float share, int rows)
        {
            if (share <= 0f) return 0;
            return Math.Min(rows, Math.Max(1, (int)Math.Round(share * rows)));
        }

        /// <summary>A triangle wave 0 to 1 to 0 at <paramref name="hz"/> cycles per second of <paramref name="time"/>; 0 when the rate is not positive.</summary>
        public static float Pulse(float time, float hz)
        {
            if (hz <= 0f) return 0f;
            var phase = time * hz - (float)Math.Floor(time * hz);
            return phase < 0.5f ? phase * 2f : 2f - phase * 2f;
        }
    }
}
