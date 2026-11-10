namespace UncannyValet.Traffic
{
    /// <summary>
    /// Starts a delivered car's exit: leave the curb along the curb exit route, yield at the merge point until the east
    /// lane has a gap, merge, drive off to the right.
    /// </summary>
    public interface IExitRouter
    {
        /// <summary>Begins an exit trip. Returns a trip id, or <see cref="TripId.None"/>.</summary>
        int BeginExit();
    }
}
