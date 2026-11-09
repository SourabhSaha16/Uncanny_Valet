namespace UncannyValet.UI
{
    /// <summary>Plain text for the results screen. Built by <see cref="ResultsFormatter"/>.</summary>
    public sealed class ResultsContent
    {
        public string Title;
        /// <summary>The stars earned, one character per possible star, such as "**-" for two of three.</summary>
        public string Stars;
        public string[] Lines;
        public string ButtonLabel;
    }
}
