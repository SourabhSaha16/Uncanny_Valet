using System;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Feeds the bar view and the active power timer from the Core events PowerBarChanged, PowerActivated and PowerEnded,
    /// and advances them with real time. The bar is visible only while Thor owns a power (IRunProgress).
    /// </summary>
    public sealed class PowerBarTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private IRunProgress _progress;
        private float _lastTime = -1f;

        public PowerBarTracker(IEventBus bus, PowerBarSettings settings, IRunProgress progress = null)
        {
            _bus = bus;
            _progress = progress;
            View = new PowerBarView(settings.FillSmoothing, settings.PulseSeconds, settings.SpentFlashSeconds);
            Active = new ActivePowers(settings);
            _bus.Subscribe<PowerBarChanged>(OnBar);
            _bus.Subscribe<PowerActivated>(OnActivated);
            _bus.Subscribe<PowerEnded>(OnEnded);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
        }

        public PowerBarView View { get; }
        public ActivePowers Active { get; }

        /// <summary>True while at least one power is owned; nothing of the bar is drawn before that.</summary>
        public bool IsVisible
        {
            get
            {
                if (_progress == null) Services.TryGet(out _progress);
                return _progress != null && _progress.EarnedPowers.Count > 0;
            }
        }

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
            _bus.Unsubscribe<PowerBarChanged>(OnBar);
            _bus.Unsubscribe<PowerActivated>(OnActivated);
            _bus.Unsubscribe<PowerEnded>(OnEnded);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
        }

        private void OnBar(PowerBarChanged e) => View.Set(e.Value, e.OpenBuckets, e.SpentBucket);

        private void OnActivated(PowerActivated e) => Active.Start(e.Power);

        private void OnEnded(PowerEnded e) => Active.End(e.Power);

        private void OnShiftEnded(ShiftEnded e) => Active.Clear();
    }
}
