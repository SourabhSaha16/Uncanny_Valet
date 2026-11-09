using System;

namespace UncannyValet.UI.Screens
{
    /// <summary>
    /// Pure timing of the results screen's stamp-style star reveal. Star <c>i</c> lands <c>FirstDelay + i * Interval</c>
    /// seconds after the screen opens: it starts big and slams down to normal size. The third star also bursts.
    /// </summary>
    public static class StarStamp
    {
        public const float FirstDelaySeconds = 0.5f;
        public const float IntervalSeconds = 0.6f;
        public const float SlamSeconds = 0.2f;
        public const float StartScale = 2f;
        public const float BurstSeconds = 0.45f;
        public const int BurstStar = 2;

        /// <summary>Seconds after the screen opened at which a star lands.</summary>
        public static float LandTime(int star) => FirstDelaySeconds + star * IntervalSeconds + SlamSeconds;

        /// <summary>Size multiplier of a star: 0 before it appears, StartScale falling to 1 as it lands, then 1.</summary>
        public static float Scale(int star, float seconds)
        {
            var start = FirstDelaySeconds + star * IntervalSeconds;
            if (seconds < start) return 0f;
            var t = Math.Min(1f, (seconds - start) / SlamSeconds);
            return StartScale + (1f - StartScale) * t;
        }

        /// <summary>0 to 1 across the burst after the third star lands; -1 when there is no burst.</summary>
        public static float BurstProgress(int star, int starsEarned, float seconds)
        {
            if (star != BurstStar || starsEarned <= BurstStar) return -1f;
            var since = seconds - LandTime(star);
            return since < 0f || since >= BurstSeconds ? -1f : since / BurstSeconds;
        }
    }
}
