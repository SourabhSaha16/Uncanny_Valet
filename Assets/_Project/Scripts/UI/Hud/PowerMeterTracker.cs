using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Feeds the meter view and the active power timers from the Core events PowerMeterChanged, PowerActivated and
    /// PowerEnded, and advances them with real time. The only thing the HUD knows about the powers module.
    /// </summary>
    public sealed class PowerMeterTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private float _lastTime = -1f;

        public PowerMeterTracker(IEventBus bus, PowerMeterSettings settings)
        {
            _bus = bus;
            View = new PowerMeterView(settings.FillSmoothing, settings.PulseSeconds);
            Active = new ActivePowers(settings);
            _bus.Subscribe<PowerMeterChanged>(OnMeter);
            _bus.Subscribe<PowerActivated>(OnActivated);
            _bus.Subscribe<PowerEnded>(OnEnded);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
        }

        public PowerMeterView View { get; }
        public ActivePowers Active { get; }

        /// <summary>Advances the animations to the given unscaled time (seconds).</summary>
        public void Tick(float time)
        {
            if (_lastTime >= 0f)
            {
                var dt = time - _lastTime;
                View.Tick(dt);
                Active.Tick(dt);
            }
            _lastTime = time;
        }

        public void Dispose()
        {
            _bus.Unsubscribe<PowerMeterChanged>(OnMeter);
            _bus.Unsubscribe<PowerActivated>(OnActivated);
            _bus.Unsubscribe<PowerEnded>(OnEnded);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
        }

        private void OnMeter(PowerMeterChanged e) => View.Set(e.Level, e.Fill01);

        private void OnActivated(PowerActivated e) => Active.Start((int)e.Power);

        private void OnEnded(PowerEnded e) => Active.End((int)e.Power);

        private void OnShiftEnded(ShiftEnded e) => Active.Clear();
    }
}
