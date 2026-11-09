namespace UncannyValet.Director
{
    /// <summary>Where the relief state machine is.</summary>
    public enum ReliefStage
    {
        None,

        /// <summary>All arrivals are held.</summary>
        Holding,

        /// <summary>Only plain cars may be sent.</summary>
        Plain
    }
}
