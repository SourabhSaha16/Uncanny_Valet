namespace UncannyValet.Core.Events
{
    /// <summary>Why the run chose its next lot. Stored in events only; append only.</summary>
    public enum RunOutcome
    {
        /// <summary>A lot was cleared: on to the next one.</summary>
        Advanced = 0,
        /// <summary>A training lot was failed: the same lot again.</summary>
        RestartedTraining = 1,
        /// <summary>A crazy lot or the finale was failed: back to Midgard's crazy lot, everything kept.</summary>
        SentBack = 2,
        /// <summary>The run is over; there is no next lot.</summary>
        RunComplete = 3,
    }
}
