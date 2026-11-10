namespace UncannyValet.Core.Contracts
{
    /// <summary>The kind of thing a director decision log entry records.</summary>
    public enum DecisionKind
    {
        Scheduled,
        ReliefStarted,
        ReliefEnded,
        DilemmaResolved
    }
}
