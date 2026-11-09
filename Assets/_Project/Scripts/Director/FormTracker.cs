using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>
    /// Rule system owning form F: a slow read (-1 struggling to +1 cruising) of the tip rate against the 3-star pace,
    /// recent angry patrons and the combo. Updated every few seconds and moves only a limited step each time.
    /// </summary>
    public sealed class FormTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly FormSettings _settings;
        private readonly IGameClock _clock;
        private readonly IShiftScoreView _score;
        private readonly int _maxCombo;
        private readonly RecentTips _tips = new RecentTips();
        private float _lastUpdate;
        private int _combo = 1;

        /// <param name="score">Supplies the 3-star score for the pace; null means the tip term is 0.</param>
        /// <param name="maxCombo">The highest combo multiplier.</param>
        public FormTracker(IEventBus bus, FormSettings settings, IGameClock clock, IShiftScoreView score, int maxCombo)
        {
            _bus = bus; _settings = settings; _clock = clock; _score = score; _maxCombo = maxCombo;
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<TipEarned>(OnTip);
            _bus.Subscribe<ScoreChanged>(OnScore);
        }

        /// <summary>Form F, -1..1. Starts each shift at 0.</summary>
        public float Value { get; private set; }

        public void Dispose()
        {
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<TipEarned>(OnTip);
            _bus.Unsubscribe<ScoreChanged>(OnScore);
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
            var rate = elapsed > 0f ? _tips.SumWithin(lot.NowSeconds, window) / elapsed : 0f;
            var length = _clock.ShiftLengthSeconds;
            var pace = _score != null && length > 0f ? _score.ThreeStarScore / length : 0f;
            return FormMath.Target(rate, pace, lot.CountAngry(window), _combo, _maxCombo, _settings);
        }

        private float NowSeconds() => _clock.ShiftLengthSeconds - _clock.SecondsRemaining;

        private void OnShiftStarted(ShiftStarted e)
        {
            Value = 0f;
            _lastUpdate = 0f;
            _combo = 1;
            _tips.Clear();
        }

        private void OnTip(TipEarned e) => _tips.Add(NowSeconds(), e.Amount);

        private void OnScore(ScoreChanged e) => _combo = e.ComboMultiplier;
    }
}
