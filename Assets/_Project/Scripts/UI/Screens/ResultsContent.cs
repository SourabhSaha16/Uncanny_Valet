namespace UncannyValet.UI.Screens
{
    /// <summary>Plain text for the results screen. Built once per shift by <see cref="ResultsFormatter"/>.</summary>
    public sealed class ResultsContent
    {
        public string Title;
        /// <summary>CLEARED or FAILED.</summary>
        public string Verdict;
        /// <summary>True when the lot was cleared (the verdict is drawn green, otherwise red).</summary>
        public bool Cleared;
        /// <summary>Blessings and the threshold, such as "12 / 30".</summary>
        public string Blessings;
        /// <summary>What is left to do, or the reached line.</summary>
        public string Goal;
        public StatLine[] Stats;
        /// <summary>The god's name for the speech panel; empty on a training lot.</summary>
        public string Speaker = string.Empty;
        /// <summary>The verdict line, already wrapped to the panel; empty when there is none.</summary>
        public string[] SpeechLines = System.Array.Empty<string>();
    }
}
