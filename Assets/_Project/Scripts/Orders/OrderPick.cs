namespace UncannyValet.Orders
{
    /// <summary>The selector's choice: this car goes to that spot.</summary>
    public readonly struct OrderPick
    {
        public readonly ParkedCar Car;
        public readonly int TargetSpot;

        public OrderPick(ParkedCar car, int targetSpot) { Car = car; TargetSpot = targetSpot; }
    }
}
