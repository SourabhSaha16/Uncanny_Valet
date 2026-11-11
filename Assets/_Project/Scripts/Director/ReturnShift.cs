using UncannyValet.Core.Ids;

namespace UncannyValet.Director
{
    /// <summary>One return the scheduler wants moved: add <see cref="DeltaSeconds"/> to the stay (negative = earlier).</summary>
    public readonly struct ReturnShift
    {
        public readonly CarId Car;
        public readonly float DeltaSeconds;
        public ReturnShift(CarId car, float deltaSeconds) { Car = car; DeltaSeconds = deltaSeconds; }
    }
}
