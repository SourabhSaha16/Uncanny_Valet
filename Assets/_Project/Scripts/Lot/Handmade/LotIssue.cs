namespace UncannyValet.Lot.Handmade
{
    /// <summary>One finding of the validator: the rule, how bad it is, what to fix and the name of the marker to look at (empty for the lot as a whole).</summary>
    public readonly struct LotIssue
    {
        public readonly LotRule Rule;
        public readonly LotSeverity Severity;
        public readonly string Message;
        public readonly string Subject;

        public LotIssue(LotRule rule, LotSeverity severity, string message, string subject = "")
        {
            Rule = rule;
            Severity = severity;
            Message = message;
            Subject = subject ?? string.Empty;
        }

        public bool IsError => Severity == LotSeverity.Error;

        public override string ToString() => "[" + (int)Rule + " " + Rule + "] " + (Subject.Length > 0 ? Subject + ": " : string.Empty) + Message;
    }
}
