namespace UncannyValet.Traffic
{
    /// <summary>
    /// Starts a patron car's entry: wait off-screen left until the east lane has a gap, drive the lane, slow down,
    /// turn up the queue entry route into the slot and stop.
    /// </summary>
    public interface IEntryRouter
    {
        /// <summary>Begins an entry trip for arrival slot <paramref name="slot"/>. Returns a trip id, or <see cref="TripId.None"/>.</summary>
        int BeginEntry(int slot);
    }
}
