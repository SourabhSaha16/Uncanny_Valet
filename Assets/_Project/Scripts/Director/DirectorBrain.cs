using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>
    /// The director's per-tick logic as a plain class: reads the lot snapshot, estimates workload, asks the pacer when
    /// a car is due and the dilemma director which car, and publishes PatronScheduled, DirectorPhaseChanged and the
    /// relief events. The MonoBehaviour shell only feeds it time.
    /// </summary>
    public sealed class DirectorBrain : IDisposable
    {
        private const float DefaultGapMin = 15f;
        private const float DefaultGapMax = 25f;
        private const int DefaultQueueCap = 3;

        private readonly IEventBus _bus;
        private readonly ITuning _tuning;
        private readonly IGameClock _clock;
        private readonly ILotSnapshotSource _lot;
        private readonly IIntensityProfile _profile;
        private readonly DirectorState _state;
        private readonly FormTracker _form;
        private readonly ArrivalPacer _pacer;
        private readonly DilemmaDirector _dilemmas;
        private readonly DirectorSettings _settings;
        private readonly ReturnDirector _returns;
        private bool _phaseKnown;
        private DirectorPhase _phase;

        public DirectorBrain(IEventBus bus, ITuning tuning, IGameClock clock, ILotSnapshotSource lot,
            IIntensityProfile profile, DirectorState state, DirectorSettings settings, FormTracker form,
            ArrivalPacer pacer, DilemmaDirector dilemmas, ReturnDirector returns = null)
        {
            _bus = bus; _tuning = tuning; _clock = clock; _lot = lot; _profile = profile; _state = state;
            _settings = settings; _form = form; _pacer = pacer; _dilemmas = dilemmas; _returns = returns;
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
            var input = BuildInput(lot, load, target, fraction, length);
            var decision = _pacer.Tick(input, dt);
            _dilemmas.Tick(lot);
            _returns?.Tick(lot, input.Gap(_settings.Pacing.FormInfluence), _pacer.InRelief, _phase);
            Publish(decision, load, target, lot, input, fraction);
        }

        // Assist mode clamps form F at 0: a cruising player is not pushed harder.
        private float EffectiveForm => _settings.Picker.Assist ? Mathf.Min(_form.Value, 0f) : _form.Value;

        private PacerInput BuildInput(in LotSnapshot lot, WorkloadReading load, float target, float fraction, float length)
        {
            var queue = 0;
            for (var i = 0; i < lot.Cars.Count; i++) if (lot.Cars[i].AwaitingPark) queue++;
            var gapScale = Read(TuningKey.PlanArrivalGapScale, 1f);
            var perSecond = Read(TuningKey.GameMinutesPerSecond, 1f);
            var toSeconds = gapScale / (perSecond > 0f ? perSecond : 1f);
            var cap = (int)Read(TuningKey.PlanMaxWaitingCars, DefaultQueueCap);
            return new PacerInput(load.W, load.P, target, EffectiveForm, lot.CountAngry(_settings.Relief.AngryWindowSeconds),
                queue, cap, _state.ArrivalsUsed, _profile.AreaFrom(fraction, length), _profile.AreaFrom(0f, length),
                Read(TuningKey.ArrivalGapMinMinutes, DefaultGapMin) * toSeconds,
                Read(TuningKey.ArrivalGapMaxMinutes, DefaultGapMax) * toSeconds, _clock.SecondsRemaining);
        }

        private void Publish(in PacerDecision d, WorkloadReading load, float target, in LotSnapshot lot,
            in PacerInput input, float fraction)
        {
            var now = lot.NowSeconds;
            _state.Workload = load.W; _state.Pressure = load.P; _state.Target = target; _state.Form = EffectiveForm;
            _state.SecondsToNextArrival = d.SecondsToNext;
            _state.ArrivalBudget = _settings.Pacing.ArrivalBudget;
            _state.ArrivalBudgetMax = _settings.Pacing.MaxArrivals;
            if (d.ReliefStarted)
            {
                _bus.Publish(new ReliefStarted(d.Trigger));
                Log(DecisionKind.ReliefStarted, now, d.Reason, d.Trigger);
            }
            if (d.ReliefEnded) EndRelief(now);
            _state.InRelief = _pacer.InRelief;
            if (d.IsSpawn) Schedule(d, lot, input, fraction);
        }

        private void Schedule(in PacerDecision d, in LotSnapshot lot, in PacerInput input, float fraction)
        {
            var gap = input.Gap(_settings.Pacing.FormInfluence);
            var context = new DilemmaContext(_profile.Weights, d.Reason, d.ForcePlain || _pacer.InRelief, gap,
                _profile.IsRising(fraction), _settings.Pacing.ArrivalBudget);
            var pick = _dilemmas.Choose(lot, context);
            _bus.Publish(new PatronScheduled(pick.Archetype, true, d.Reason));
            _state.Log(new DirectorDecision(DecisionKind.Scheduled, lot.NowSeconds, d.Reason, default, pick.Archetype, true,
                _state.ArrivalsUsed, _state.Workload, _state.Target, pick.HasRule, pick.Rule, pick.Multiplier));
        }

        private void EndRelief(float now)
        {
            _bus.Publish(new ReliefEnded());
            Log(DecisionKind.ReliefEnded, now, ScheduleReason.OnTarget, default);
        }

        private void PublishPhase(DirectorPhase phase)
        {
            if (_phaseKnown && phase == _phase) return;
            _phaseKnown = true;
            _phase = phase;
            _state.Phase = phase;
            _bus.Publish(new DirectorPhaseChanged(phase));
        }

        private void Log(DecisionKind kind, float now, ScheduleReason reason, ReliefTrigger trigger)
            => _state.Log(new DirectorDecision(kind, now, reason, trigger, CarArchetype.Plain, false,
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
