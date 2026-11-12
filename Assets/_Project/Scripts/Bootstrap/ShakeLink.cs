using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Bootstrap
{
    /// <summary>Turns a hard hit on the car the player is driving into a screen shake, sized by the live tuning numbers.</summary>
    public sealed class ShakeLink : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ITuning _tuning;
        private readonly Action<float, float> _shake;

        /// <param name="shake">Receives the shake size (world units) and length (seconds).</param>
        public ShakeLink(IEventBus bus, ITuning tuning, Action<float, float> shake)
        {
            _bus = bus;
            _tuning = tuning;
            _shake = shake;
            _bus.Subscribe<CarImpact>(OnImpact);
        }

        /// <summary>Stops listening.</summary>
        public void Dispose() => _bus.Unsubscribe<CarImpact>(OnImpact);

        private void OnImpact(CarImpact e)
        {
            if (!e.IsPlayerCar) return;
            if (!_tuning.TryGet(TuningKey.DamageMinForce, out var min)) return;
            var size = CameraShake.Amplitude(e.Force, min, _tuning.Get(TuningKey.ShakeFullForce), _tuning.Get(TuningKey.ShakeStrength));
            if (size > 0f) _shake(size, _tuning.Get(TuningKey.ShakeSeconds));
        }
    }
}
