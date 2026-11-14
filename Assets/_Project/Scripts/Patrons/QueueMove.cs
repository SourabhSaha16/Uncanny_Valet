using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>One queued car told to drive up to a lower slot.</summary>
    public readonly struct QueueMove
    {
        public readonly CarId Car;
        public readonly int ToSlot;
        public QueueMove(CarId car, int toSlot) { Car = car; ToSlot = toSlot; }
    }
}
