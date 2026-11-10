namespace UncannyValet.Traffic
{
    /// <summary>Where a patron car is in its entry or exit trip.</summary>
    public enum TripPhase
    {
        /// <summary>Entry only: off-screen at the lane start, waiting for a gap.</summary>
        Waiting,
        /// <summary>Driving the east lane with car following.</summary>
        OnLane,
        /// <summary>Driving a lot route (queue entry or curb exit).</summary>
        OnRoute,
        /// <summary>Exit only: stopped at the merge point, waiting for a gap.</summary>
        Yielding,
        /// <summary>The trip is over; this is the final pose.</summary>
        Finished,
    }
}
