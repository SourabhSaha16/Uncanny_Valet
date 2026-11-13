using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure layout of the blessing bar: it spans 0 to the lot's blessing threshold, so the threshold marker sits at the
    /// right end. The fill is in whole pixels from the left end of the bar.
    /// </summary>
    public readonly struct BlessingBarLayout
    {
        /// <summary>Filled pixels of the bar.</summary>
        public readonly int Fill;

        /// <summary>True once the blessings have reached the threshold.</summary>
        public readonly bool Reached;

        public BlessingBarLayout(int blessings, int threshold, int widthPixels)
        {
            var top = Math.Max(1, threshold);
            var clamped = Math.Max(0, blessings);
            Fill = (int)((long)Math.Min(clamped, top) * widthPixels / top);
            Reached = clamped >= top;
        }
    }
}
