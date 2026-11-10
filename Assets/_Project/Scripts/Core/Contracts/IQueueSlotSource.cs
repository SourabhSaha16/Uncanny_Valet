namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// Decides which arrival queue slot a driving-in patron car should take right now. Implemented by the Patrons module;
    /// an entry trip asks it while it drives, so a slot that frees up after the car was sent is used.
    /// </summary>
    public interface IQueueSlotSource
    {
        /// <summary>
        /// The slot the car that is currently heading for <paramref name="currentSlot"/> should head for now: a free slot
        /// nearer the curb (a lower index), or <paramref name="currentSlot"/> when nothing better is available.
        /// </summary>
        int Reevaluate(int currentSlot);
    }
}
