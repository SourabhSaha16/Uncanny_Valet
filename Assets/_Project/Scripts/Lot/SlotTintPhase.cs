namespace UncannyValet.Lot
{
    /// <summary>What a service slot is doing, as far as its glow is concerned.</summary>
    public enum SlotTintPhase
    {
        /// <summary>Nothing is serviced here; normal slot colour.</summary>
        Idle,

        /// <summary>A car is being washed or charged; the slot is lit.</summary>
        Working,

        /// <summary>The service just finished; the glow fades out.</summary>
        Done
    }
}
