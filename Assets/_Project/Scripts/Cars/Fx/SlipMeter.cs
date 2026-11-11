using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;

namespace UncannyValet.Cars.Fx
{
    /// <summary>
    /// Holds the slide of the car the player drives, for the tyre sound to poll through <see cref="ISlipSource"/>. Only one car
    /// has a human driver at a time; the latest reporter owns the value and only its owner can clear it.
    /// </summary>
    public sealed class SlipMeter : ISlipSource
    {
        private CarId _owner;

        public bool IsSliding { get; private set; }
        public float Intensity01 { get; private set; }

        /// <summary>Sets the current slide of <paramref name="car"/>.</summary>
        public void Report(CarId car, bool sliding, float intensity01)
        {
            _owner = car;
            IsSliding = sliding;
            Intensity01 = sliding ? intensity01 : 0f;
        }

        /// <summary>Clears the slide if <paramref name="car"/> is the one reporting.</summary>
        public void Clear(CarId car)
        {
            if (_owner != car) return;
            IsSliding = false;
            Intensity01 = 0f;
        }
    }
}
