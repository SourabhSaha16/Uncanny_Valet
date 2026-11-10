namespace UncannyValet.Core.Events
{
    /// <summary>Which deadline a ticket timer counts down.</summary>
    public enum TimerKind
    {
        /// <summary>Bring the requested car back to the curb, or the tip is $0.</summary>
        Pickup
    }
}
