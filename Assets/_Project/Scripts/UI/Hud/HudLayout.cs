namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure pixel layout of the two small corner panels on the 480x270 canvas: the blessing count with its bar and
    /// threshold at the top left and the favour badge at the top right. Offsets are from the panel's top-left corner.
    /// </summary>
    public static class HudLayout
    {
        /// <summary>Gap between a panel and the canvas edge.</summary>
        public const int Margin = 4;
        /// <summary>Scale of the blessing and favour text (the pixel font cannot go below 1).</summary>
        public const int TextScale = 1;

        public const int BlessingsWidth = 64;
        public const int BlessingsHeight = 40;
        public const int BoltX = 5;
        public const int BoltY = 4;
        public const int CountX = 18;
        public const int CountY = 7;
        public const int BarX = 6;
        public const int BarY = 20;
        public const int BarWidth = 50;
        public const int BarHeight = 5;
        /// <summary>The threshold marker is a tick at the right end of the bar, this many pixels wide.</summary>
        public const int MarkerWidth = 2;
        /// <summary>The marker sticks out this many pixels above and below the bar.</summary>
        public const int MarkerOverhang = 2;
        public const int GoalTextY = 29;

        public const int FavourWidth = 26;
        public const int FavourHeight = 16;
        public const int FavourTextY = 4;
    }
}
