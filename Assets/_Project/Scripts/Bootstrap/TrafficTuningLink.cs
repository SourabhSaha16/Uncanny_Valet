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
        private readonly Action<float> _setEntrySpeed;
        private readonly Action<float> _setExitSpeed;

        /// <param name="setAmbientRate">Receives the ambient cars per minute.</param>
        /// <param name="setSpeedScale">Receives the ambient speed multiplier.</param>
        /// <param name="setEntrySpeed">Receives the speed of patron cars driving in (units per second); null ignores the key.</param>
        /// <param name="setExitSpeed">Receives the speed of delivered cars driving off (units per second); null ignores the key.</param>
        public TrafficTuningLink(IEventBus bus, Action<float> setAmbientRate, Action<float> setSpeedScale,
            Action<float> setEntrySpeed = null, Action<float> setExitSpeed = null)
        {
            _bus = bus;
            _setAmbientRate = setAmbientRate;
            _setSpeedScale = setSpeedScale;
            _setEntrySpeed = setEntrySpeed;
            _setExitSpeed = setExitSpeed;
            _bus.Subscribe<TuningChanged>(OnChanged);
        }

        /// <summary>Stops forwarding.</summary>
        public void Dispose() => _bus.Unsubscribe<TuningChanged>(OnChanged);

        private void OnChanged(TuningChanged e)
        {
            switch (e.Key)
            {
                case TuningKey.TrafficAmbientPerMinute: _setAmbientRate(e.Value); break;
                case TuningKey.TrafficSpeedScale: _setSpeedScale(e.Value); break;
                case TuningKey.TrafficEntrySpeed: _setEntrySpeed?.Invoke(e.Value); break;
                case TuningKey.TrafficExitSpeed: _setExitSpeed?.Invoke(e.Value); break;
            }
        }
    }
}
