namespace UncannyValet.Powers
{
    /// <summary>Decides which bar gains are worth a +X popup.</summary>
    public static class PowerFillRule
    {
        /// <summary>Smallest gain (on the 0 to 100 bar) that is reported; anything less would show as +0.</summary>
        public const float MinReported = 0.5f;

        /// <summary>True when a gain of <paramref name="taken"/> should be published.</summary>
        public static bool IsWorthShowing(float taken) => taken >= MinReported;
    }
}
