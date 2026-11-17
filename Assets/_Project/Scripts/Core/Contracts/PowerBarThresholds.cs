namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// The one list of power bar level thresholds (bar value 0 to 100). The power bar uses it to decide what is usable and the
    /// HUD uses it to place the lightning-bolt markers, so a marker sits exactly where its power becomes usable.
    /// </summary>
    public static class PowerBarThresholds
    {
        /// <summary>The full bar.</summary>
        public const float Full = 100f;

        /// <summary>Number of levels (markers).</summary>
        public const int Count = 3;

        private static readonly float[] Values = { 33.3f, 66.7f, 100f };

        /// <summary>Bar value at which <paramref name="level"/> (1 to 3) opens.</summary>
        public static float ValueOf(int level) => Values[level - 1];

        /// <summary>Where <paramref name="level"/> (1 to 3) sits on the bar, 0 to 1: the same number the bar fill uses.</summary>
        public static float FractionOf(int level) => Values[level - 1] / Full;
    }
}
