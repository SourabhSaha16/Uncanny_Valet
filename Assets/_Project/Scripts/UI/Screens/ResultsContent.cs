namespace UncannyValet.UI.Screens
{
    /// <summary>Plain text for the results screen. Built once per shift by <see cref="ResultsFormatter"/>.</summary>
    public sealed class ResultsContent
    {
        public string Title;
        /// <summary>Total tips, such as "$320".</summary>
        public string Score;
        /// <summary>Stars earned, 0 to 3.</summary>
        public int Stars;
        /// <summary>What to aim for next, or the top-marks line.</summary>
        public string Goal;
        public StatLine[] Stats;
    }
}
