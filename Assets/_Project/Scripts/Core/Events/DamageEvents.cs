using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Damage events, grouped in one file by domain (the one allowed exception to one type per file).

    /// <summary>
    /// A car took damage from a hard impact (published by Cars). Damage01 is the car's total damage afterwards (0..1,
    /// 1 = wrecked) and Delta01 what this hit added. Patrons keeps it for the tip; the HUD shows it on the ticket.
    /// </summary>
    public readonly struct CarDamaged : IGameEvent
    {
        public readonly CarId Car;
        public readonly float Damage01;
        public readonly float Delta01;
        public CarDamaged(CarId car, float damage01, float delta01) { Car = car; Damage01 = damage01; Delta01 = delta01; }
    }

    /// <summary>
    /// A car took a hard impact (a wall, a station edge or another car; never a pedestrian or a gentle tap), published by Cars
    /// for every car involved. Force is the speed change the hit forced on the car, in units per second. Screen shake and
    /// sounds can listen to it; IsPlayerCar is true when the valet is driving that car.
    /// </summary>
    public readonly struct CarImpact : IGameEvent
    {
        public readonly CarId Car;
        public readonly float Force;
        public readonly bool IsPlayerCar;
        public CarImpact(CarId car, float force, bool isPlayerCar) { Car = car; Force = force; IsPlayerCar = isPlayerCar; }
    }
}
