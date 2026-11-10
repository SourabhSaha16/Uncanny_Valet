namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure pixel layout of the two small corner panels on the 480x270 canvas (Revision 5): the coin score with the
    /// three-star bar at the top left and the combo badge at the top right. Offsets are from the panel's top-left corner.
    /// </summary>
    public static class HudLayout
    {
        /// <summary>Gap between a panel and the canvas edge.</summary>
        public const int Margin = 4;
        /// <summary>Scale of the score and combo text (the pixel font cannot go below 1).</summary>
        public const int TextScale = 1;
        /// <summary>Distance from the star marker's bar position to its icon's left edge (about half the icon).</summary>
        public const int StarOverhang = 4;

        public const int ScoreWidth = 64;
        public const int ScoreHeight = 28;
        public const int CoinX = 5;
        public const int CoinY = 4;
        public const int TipsX = 16;
        public const int TipsY = 5;
        public const int BarX = 6;
        public const int BarY = 17;
        public const int BarWidth = 48;
        public const int BarHeight = 4;

        public const int ComboWidth = 26;
        public const int ComboHeight = 16;
        public const int ComboTextY = 4;

        /// <summary>Left edge, from the score panel's left, of the icon of a star marker (0 to 2).</summary>
        public static int StarX(StarBarLayout bar, int star) => BarX + bar.MarkerX(star) - StarOverhang;
    }
}
