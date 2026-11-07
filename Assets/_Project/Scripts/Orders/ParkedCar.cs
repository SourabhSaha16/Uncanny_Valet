using UncannyValet.Core.Ids;

namespace UncannyValet.Orders
{
    /// <summary>A car that could be given an order: parked in a spot and showing a letter.</summary>
    public readonly struct ParkedCar
    {
        public readonly CarId Car;
        public readonly char Letter;
        public readonly int Spot;

        public ParkedCar(CarId car, char letter, int spot) { Car = car; Letter = letter; Spot = spot; }
    }
}
