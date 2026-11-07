using UncannyValet.Core.Ids;

namespace UncannyValet.Orders
{
    /// <summary>One open boss order: put <see cref="Car"/> in <see cref="TargetSpot"/>.</summary>
    public readonly struct Order
    {
        public readonly OrderId Id;
        public readonly CarId Car;
        public readonly char Letter;
        public readonly int TargetSpot;
        /// <summary>Game minute of the night when the order was issued.</summary>
        public readonly float IssuedAtMinute;

        public Order(OrderId id, CarId car, char letter, int targetSpot, float issuedAtMinute)
        {
            Id = id; Car = car; Letter = letter; TargetSpot = targetSpot; IssuedAtMinute = issuedAtMinute;
        }
    }
}
