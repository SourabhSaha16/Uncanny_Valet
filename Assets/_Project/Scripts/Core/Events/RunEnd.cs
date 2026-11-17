namespace UncannyValet.Core.Events
{
    /// <summary>How a completed run ended. Append only.</summary>
    public enum RunEnd
    {
        /// <summary>The run is not complete.</summary>
        None = 0,
        /// <summary>The Asgard finale was cleared.</summary>
        Finale = 1,
        /// <summary>The last lot the run definition lists was cleared before the finale exists (the demo end).</summary>
        EndOfContent = 2,
    }
}
