namespace UncannyValet.Core.Contracts
{
    /// <summary>Owner of the lot snapshot (the Director module's LotStateTracker). The only writer of that state.</summary>
    public interface ILotSnapshotSource
    {
        /// <summary>Rebuilds and returns the snapshot of the lot right now. Allocation-free in steady state.</summary>
        LotSnapshot Capture();
    }
}
