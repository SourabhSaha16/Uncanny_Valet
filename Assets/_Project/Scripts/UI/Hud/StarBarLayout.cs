using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure layout of the three-star progress bar: the bar spans 0 to the three-star score; each star marker sits at
    /// its threshold. Pixel positions are offsets from the left end of the bar.
    /// </summary>
    public readonly struct StarBarLayout
    {
        public const int Stars = 3;

        /// <summary>Filled pixels of the bar.</summary>
        public readonly int Fill;
        /// <summary>Stars reached so far, 0 to 3.</summary>
        public readonly int LitStars;
        private readonly int _marker0;
        private readonly int _marker1;
        private readonly int _width;

        public StarBarLayout(int score, int oneStar, int twoStar, int threeStar, int widthPixels)
        {
            var top = Math.Max(1, threeStar);
            var clamped = Math.Max(0, score);
            _width = widthPixels;
            Fill = (int)((long)Math.Min(clamped, top) * widthPixels / top);
            _marker0 = At(oneStar, top, widthPixels);
            _marker1 = At(twoStar, top, widthPixels);
            LitStars = clamped >= threeStar ? 3 : clamped >= twoStar ? 2 : clamped >= oneStar ? 1 : 0;
        }

        /// <summary>Pixel offset of a star marker (0 to 2), the last at the right end of the bar.</summary>
        public int MarkerX(int star) => star <= 0 ? _marker0 : star == 1 ? _marker1 : _width;

        public bool IsLit(int star) => star < LitStars;

        private static int At(int threshold, int top, int width)
            => (int)((long)Math.Min(Math.Max(0, threshold), top) * width / top);
    }
}
