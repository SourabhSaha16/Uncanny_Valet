namespace UncannyValet.Core.Events
{
    /// <summary>Which deadline a ticket timer counts down.</summary>
    public enum TimerKind
    {
        /// <summary>Get the waiting car parked, or it vanishes.</summary>
        Park,

        /// <summary>Bring the requested car back to the curb, or the tip is $0.</summary>
        Pickup
    }
}
