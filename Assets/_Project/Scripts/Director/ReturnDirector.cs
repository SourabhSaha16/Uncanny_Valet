using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>
    /// Runs the <see cref="ReturnScheduler"/> once per director tick: builds its input from the lot and the pacing
    /// numbers and publishes a <see cref="ReturnShiftRequested"/> for each return it wants moved. The Patrons module
    /// executes them and answers with <see cref="PatronReturnShifted"/>, which the lot tracker folds into the next
    /// snapshot. Remembers when a patron was last sent to the curb, for the pickup spacing.
    /// </summary>
    public sealed class ReturnDirector : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly ITuning _tuning;
        private readonly DirectorSettings _settings;
        private readonly ReturnScheduler _scheduler;
        private readonly List<ReturnShift> _shifts = new List<ReturnShift>();
        private float _lastSummoned = float.NegativeInfinity;

        public ReturnDirector(IEventBus bus, IGameClock clock, ITuning tuning, DirectorSettings settings)
        {
            _bus = bus; _clock = clock; _tuning = tuning; _settings = settings;
            _scheduler = new ReturnScheduler(settings.Returns);
            _bus.Subscribe<PatronSummoned>(OnSummoned);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<PatronSummoned>(OnSummoned);
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
        }

        /// <summary>One tick. <paramref name="gap"/> is the pacing gap (workload minus target).</summary>
        public void Tick(in LotSnapshot lot, float gap, bool inRelief, DirectorPhase phase)
        {
            var rules = _settings.Rules;
            var wave = phase == DirectorPhase.Peak || phase == DirectorPhase.HarderPeak;
            var input = new ReturnInput(lot.NowSeconds, _clock.SecondsRemaining, gap, _settings.Pacing.Band, inRelief,
                lot.FreeSpots <= rules.LotTightSpots, wave, _lastSummoned);
            _shifts.Clear();
            if (_scheduler.Plan(lot, input, _shifts) == 0) return;
            var perSecond = _tuning.TryGet(TuningKey.GameMinutesPerSecond, out var v) && v > 0f ? v : 1f;
            for (var i = 0; i < _shifts.Count; i++)
                _bus.Publish(new ReturnShiftRequested(_shifts[i].Car, _shifts[i].DeltaSeconds * perSecond));
        }

        private void OnSummoned(PatronSummoned e) => _lastSummoned = _clock.ShiftLengthSeconds - _clock.SecondsRemaining;

        private void OnShiftStarted(ShiftStarted e)
        {
            _lastSummoned = float.NegativeInfinity;
            _scheduler.Reset();
        }
    }
}
