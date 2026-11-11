using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Bootstrap
{
    /// <summary>Forwards the traffic tuning keys from the bus to the traffic module's setters.</summary>
    public sealed class TrafficTuningLink : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly Action<float> _setMinOnScreen;
        private readonly Action<float> _setSpeedScale;
        private readonly Action<float> _setEntrySpeed;
        private readonly Action<float> _setExitSpeed;
        private readonly Action<float> _setTargetOnScreen;

        /// <param name="setMinOnScreen">Receives the fewest ambient cars kept on screen.</param>
        /// <param name="setSpeedScale">Receives the ambient speed multiplier.</param>
        /// <param name="setEntrySpeed">Receives the speed of patron cars driving in (units per second); null ignores the key.</param>
        /// <param name="setExitSpeed">Receives the speed of delivered cars driving off (units per second); null ignores the key.</param>
        /// <param name="setTargetOnScreen">Receives the ambient cars on screen that spawning aims for; null ignores the key.</param>
        public TrafficTuningLink(IEventBus bus, Action<float> setMinOnScreen, Action<float> setSpeedScale,
            Action<float> setEntrySpeed = null, Action<float> setExitSpeed = null, Action<float> setTargetOnScreen = null)
        {
            _bus = bus;
            _setMinOnScreen = setMinOnScreen;
            _setTargetOnScreen = setTargetOnScreen;
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
                case TuningKey.TrafficMinOnScreen: _setMinOnScreen(e.Value); break;
                case TuningKey.TrafficTargetOnScreen: _setTargetOnScreen?.Invoke(e.Value); break;
                case TuningKey.TrafficSpeedScale: _setSpeedScale(e.Value); break;
                case TuningKey.TrafficEntrySpeed: _setEntrySpeed?.Invoke(e.Value); break;
                case TuningKey.TrafficExitSpeed: _setExitSpeed?.Invoke(e.Value); break;
            }
        }
    }
}
