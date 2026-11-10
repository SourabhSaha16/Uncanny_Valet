using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Bootstrap
{
    /// <summary>Forwards the traffic tuning keys from the bus to the traffic module's setters.</summary>
    public sealed class TrafficTuningLink : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly Action<float> _setAmbientRate;
        private readonly Action<float> _setSpeedScale;

        /// <param name="setAmbientRate">Receives the ambient cars per minute.</param>
        /// <param name="setSpeedScale">Receives the ambient speed multiplier.</param>
        public TrafficTuningLink(IEventBus bus, Action<float> setAmbientRate, Action<float> setSpeedScale)
        {
            _bus = bus;
            _setAmbientRate = setAmbientRate;
            _setSpeedScale = setSpeedScale;
            _bus.Subscribe<TuningChanged>(OnChanged);
        }

        /// <summary>Stops forwarding.</summary>
        public void Dispose() => _bus.Unsubscribe<TuningChanged>(OnChanged);

        private void OnChanged(TuningChanged e)
        {
            if (e.Key == TuningKey.TrafficAmbientPerMinute) _setAmbientRate(e.Value);
            else if (e.Key == TuningKey.TrafficSpeedScale) _setSpeedScale(e.Value);
        }
    }
}
