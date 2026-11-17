namespace UncannyValet.UI.Hud
{
    /// <summary>Pure placement of the shift clock tab: flush under the score panel at the left edge, clear of the lot centre and the right-hand power column.</summary>
    public static class ClockLayout
    {
        /// <summary>The tab is as wide as the score panel above it, so the two read as one stacked tab.</summary>
        public const int Width = HudLayout.BlessingsWidth;
        public const int Height = 22;

        /// <summary>Left edge of the clock tab.</summary>
        public static int X(int insetPixels) => HudLayout.Margin + insetPixels;

        /// <summary>Top edge of the clock tab: just below the score panel (which starts one pixel above the margin).</summary>
        public static int Y(int gapPixels) => HudLayout.Margin - 1 + HudLayout.BlessingsHeight + gapPixels;

        /// <summary>Top of the text inside the tab, vertically centred.</summary>
        public static int TextY(int gapPixels, int textHeight) => Y(gapPixels) + (Height - textHeight) / 2;
    }
}
