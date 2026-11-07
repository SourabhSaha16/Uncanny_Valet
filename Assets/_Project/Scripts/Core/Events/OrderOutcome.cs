namespace UncannyValet.Core.Events
{
    /// <summary>How a boss order ended.</summary>
    public enum OrderOutcome
    {
        /// <summary>The car was in the ordered spot at some point after the order was issued. No reward.</summary>
        Passed,
        /// <summary>The car was handed back to its patron without having been in the ordered spot. A strike.</summary>
        Failed,
        /// <summary>The car left some other way (vanished, night ended). Nothing is judged, no strike.</summary>
        Cancelled,
    }
}
