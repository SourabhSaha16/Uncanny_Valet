namespace UncannyValet.UI
{
    /// <summary>Plain text for the night summary card. Built by <see cref="SummaryCardFormatter"/>.</summary>
    public sealed class SummaryCardContent
    {
        public string Title;
        /// <summary>Big result line on the final night; empty otherwise.</summary>
        public string Headline;
        public string[] Lines;
        /// <summary>The tip calculation rows, shown under <see cref="SummaryCardFormatter.CalcHeader"/>.</summary>
        public string[] CalcLines = new string[0];
        /// <summary>"Tomorrow: ..." text; empty when there is nothing new (and always on the final night).</summary>
        public string Tomorrow = string.Empty;
        public string ButtonLabel;
        public int Night;

        public bool HasHeadline => !string.IsNullOrEmpty(Headline);
        public bool HasCalc => CalcLines != null && CalcLines.Length > 0;
        public bool HasTomorrow => !string.IsNullOrEmpty(Tomorrow);
    }
}
