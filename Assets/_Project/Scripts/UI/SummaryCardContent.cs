namespace UncannyValet.UI
{
    /// <summary>Plain text for the night summary card. Built by <see cref="SummaryCardFormatter"/>.</summary>
    public sealed class SummaryCardContent
    {
        public string Title;
        /// <summary>Big result line on the final night; empty otherwise.</summary>
        public string Headline;
        public string[] Lines;
        public string ButtonLabel;
        public int Night;

        public bool HasHeadline => !string.IsNullOrEmpty(Headline);
    }
}
