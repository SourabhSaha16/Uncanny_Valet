namespace UncannyValet.UI.Screens
{
    /// <summary>The words under the results numbers: a god's speech on a crazy lot or the finale, a generic trainer line on a training lot.</summary>
    public readonly struct VerdictSpeech
    {
        private readonly string _speaker;
        private readonly string _line;

        public VerdictSpeech(string speaker, string line)
        {
            _speaker = speaker;
            _line = line;
        }

        /// <summary>The god's name; empty for a trainer line (no god panel).</summary>
        public string Speaker => _speaker ?? string.Empty;

        /// <summary>The line; empty when the text has no entry for the lot.</summary>
        public string Line => _line ?? string.Empty;

        /// <summary>True when the god panel (name and line) is drawn.</summary>
        public bool HasGod => Speaker.Length > 0 && Line.Length > 0;
    }
}
