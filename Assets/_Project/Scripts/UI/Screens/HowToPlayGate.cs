namespace UncannyValet.UI.Screens
{
    /// <summary>
    /// Decides whether the how-to-play screen is shown: once per page load, after the first press on the title
    /// screen. A plain in-memory flag, nothing stored, so a fresh page load shows it again and retries never do.
    /// </summary>
    public sealed class HowToPlayGate
    {
        private bool _shown;

        /// <summary>True when the screen has already been shown this session.</summary>
        public bool WasShown => _shown;

        /// <summary>Asks to show the screen: true the first time only (and records it), false afterwards.</summary>
        public bool TryShow()
        {
            if (_shown) return false;
            _shown = true;
            return true;
        }
    }
}
