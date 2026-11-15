namespace UncannyValet.Patrons
{
    /// <summary>Training lots: the Patron Director (or the fixed timer) decides when cars arrive; nothing tops up the queue.</summary>
    public sealed class DirectorArrivalPacing : IArrivalPacing
    {
        public bool TopsUpQueue => false;

        public bool Due(float elapsedMinutes) => false;

        public void Spawned(float elapsedMinutes) { }

        public void Reset() { }
    }
}
