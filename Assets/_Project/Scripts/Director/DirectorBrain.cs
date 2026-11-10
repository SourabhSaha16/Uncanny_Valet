using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>
    /// The director's per-tick logic as a plain class: reads the lot snapshot, estimates workload, asks the pacer, and
    /// publishes PatronScheduled, DirectorPhaseChanged and the relief events. The MonoBehaviour shell only feeds it time.
    /// </summary>
    public sealed class DirectorBrain : IDisposable
    {
        private const float DefaultGapMin = 15f;
        private const float DefaultGapMax = 25f;
        private const int DefaultQueueCap = 4;

        private readonly IEventBus _bus;
        private readonly ITuning _tuning;
        private readonly IGameClock _clock;
        private readonly ILotSnapshotSource _lot;
        private readonly DirectorProfile _profile;
        private readonly DirectorState _state;
        private readonly FormTracker _form;
        private readonly ArrivalPacer _pacer;
        private readonly DirectorSettings _settings;
        private bool _phaseKnown;
        private DirectorPhase _phase;

        public DirectorBrain(IEventBus bus, ITuning tuning, IGameClock clock, ILotSnapshotSource lot,
            DirectorProfile profile, DirectorState state, DirectorSettings settings, FormTracker form, ArrivalPacer pacer)
        {
            _bus = bus; _tuning = tuning; _clock = clock; _lot = lot; _profile = profile; _state = state;
            _settings = settings; _form = form; _pacer = pacer;
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<CarSpawned>(OnCarSpawned);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<CarSpawned>(OnCarSpawned);
        }

        /// <summary>One director tick. <paramref name="dt"/> is the game seconds since the last tick.</summary>
        public void Tick(float dt)
        {
            _settings.Refresh(_tuning);
            _state.Active = _settings.Enabled;
            if (!_settings.Enabled || !_clock.IsRunning) return;
            var lot = _lot.Capture();
            var load = WorkloadEstimator.Estimate(lot, _settings.Workload);
            var length = _clock.ShiftLengthSeconds;
            var fraction = length > 0f ? lot.NowSeconds / length : 0f;
            var target = _profile.Sample(fraction);
            _form.Tick(lot);
            PublishPhase(_profile.PhaseAt(fraction));
            var decision = _pacer.Tick(BuildInput(lot, load, target, fraction, length), dt);
            Publish(decision, load, target, lot.NowSeconds);
        }

        private PacerInput BuildInput(in LotSnapshot lot, WorkloadReading load, float target, float fraction, float length)
        {
            var queue = 0;
            for (var i = 0; i < lot.Cars.Count; i++) if (lot.Cars[i].AwaitingPark) queue++;
            var gapScale = Read(TuningKey.PlanArrivalGapScale, 1f);
            var perSecond = Read(TuningKey.GameMinutesPerSecond, 1f);
            var toSeconds = gapScale / (perSecond > 0f ? perSecond : 1f);
            var cap = (int)Read(TuningKey.PlanMaxWaitingCars, DefaultQueueCap);
            return new PacerInput(load.W, load.P, target, _form.Value, lot.CountAngry(_settings.Relief.AngryWindowSeconds),
                queue, cap, _state.ArrivalsUsed, _profile.AreaFrom(fraction, length), _profile.AreaFrom(0f, length),
                Read(TuningKey.ArrivalGapMinMinutes, DefaultGapMin) * toSeconds,
                Read(TuningKey.ArrivalGapMaxMinutes, DefaultGapMax) * toSeconds);
        }

        private void Publish(in PacerDecision d, WorkloadReading load, float target, float now)
        {
            _state.Workload = load.W; _state.Pressure = load.P; _state.Target = target; _state.Form = _form.Value;
            _state.SecondsToNextArrival = d.SecondsToNext;
            _state.ArrivalBudget = _settings.Pacing.ArrivalBudget;
            _state.ArrivalBudgetMax = _settings.Pacing.MaxArrivals;
            if (d.ReliefStarted)
            {
                _bus.Publish(new ReliefStarted(d.Trigger));
                Log(DecisionKind.ReliefStarted, now, d.Reason, d.Trigger, false);
            }
            if (d.ReliefEnded) EndRelief(now);
            _state.InRelief = _pacer.InRelief;
            if (!d.IsSpawn) return;
            _bus.Publish(new PatronScheduled(CarArchetype.Plain, d.ForcePlain, d.Reason));
            Log(DecisionKind.Scheduled, now, d.Reason, default, d.ForcePlain);
        }

        private void EndRelief(float now)
        {
            _bus.Publish(new ReliefEnded());
            Log(DecisionKind.ReliefEnded, now, ScheduleReason.OnTarget, default, false);
        }

        private void PublishPhase(DirectorPhase phase)
        {
            if (_phaseKnown && phase == _phase) return;
            _phaseKnown = true;
            _phase = phase;
            _state.Phase = phase;
            _bus.Publish(new DirectorPhaseChanged(phase));
        }

        private void Log(DecisionKind kind, float now, ScheduleReason reason, ReliefTrigger trigger, bool plain)
            => _state.Log(new DirectorDecision(kind, now, reason, trigger, CarArchetype.Plain, plain,
                _state.ArrivalsUsed, _state.Workload, _state.Target));

        private float Read(TuningKey key, float fallback) => TuningRead.Float(_tuning, key, fallback);

        private void OnShiftStarted(ShiftStarted e)
        {
            _pacer.Reset();
            _state.Reset();
            _phaseKnown = false;
        }

        private void OnCarSpawned(CarSpawned e)
        {
            _state.ArrivalsUsed++;
            var now = _clock.ShiftLengthSeconds - _clock.SecondsRemaining;
            if (_pacer.NotifySpawned()) EndRelief(now);
            _state.InRelief = _pacer.InRelief;
        }
    }
}
