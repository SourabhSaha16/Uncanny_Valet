using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>
    /// Rule system owning form F: a slow read (-1 struggling to +1 cruising) of the blessing rate against the pace target,
    /// recent angry patrons and the favour. Updated every few seconds and moves only a limited step each time.
    /// </summary>
    public sealed class FormTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly FormSettings _settings;
        private readonly IGameClock _clock;
        private readonly int _paceBlessings;
        private readonly int _maxFavour;
        private readonly RecentBlessings _blessings = new RecentBlessings();
        private float _lastUpdate;
        private int _favour = 1;

        /// <param name="paceBlessings">Blessings per shift that count as exactly on pace; 0 makes the blessing term 0.</param>
        /// <param name="maxFavour">The highest favour multiplier.</param>
        public FormTracker(IEventBus bus, FormSettings settings, IGameClock clock, int paceBlessings, int maxFavour)
        {
            _bus = bus; _settings = settings; _clock = clock; _paceBlessings = paceBlessings; _maxFavour = maxFavour;
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<BlessingEarned>(OnBlessing);
            _bus.Subscribe<BlessingsChanged>(OnScore);
        }

        /// <summary>Form F, -1..1. Starts each shift at 0.</summary>
        public float Value { get; private set; }

        public void Dispose()
        {
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<BlessingEarned>(OnBlessing);
            _bus.Unsubscribe<BlessingsChanged>(OnScore);
        }

        /// <summary>Call every director tick; recomputes form when the update interval has passed.</summary>
        public void Tick(in LotSnapshot lot)
        {
            var now = lot.NowSeconds;
            if (now - _lastUpdate < _settings.UpdateSeconds) return;
            _lastUpdate = now;
            var target = now < _settings.WarmupSeconds ? 0f : TargetNow(lot);
            Value = FormMath.Step(Value, target, _settings.MaxStep);
        }

        private float TargetNow(in LotSnapshot lot)
        {
            var window = _settings.WindowSeconds;
            var elapsed = lot.NowSeconds < window ? lot.NowSeconds : window;
            var rate = elapsed > 0f ? _blessings.SumWithin(lot.NowSeconds, window) / elapsed : 0f;
            var length = _clock.ShiftLengthSeconds;
            var pace = length > 0f ? _paceBlessings / length : 0f;
            return FormMath.Target(rate, pace, lot.CountAngry(window), _favour, _maxFavour, _settings);
        }

        private float NowSeconds() => _clock.ShiftLengthSeconds - _clock.SecondsRemaining;

        private void OnShiftStarted(ShiftStarted e)
        {
            Value = 0f;
            _lastUpdate = 0f;
            _favour = 1;
            _blessings.Clear();
        }

        private void OnBlessing(BlessingEarned e) => _blessings.Add(NowSeconds(), e.Amount);

        private void OnScore(BlessingsChanged e) => _favour = e.FavourMultiplier;
    }
}
