using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Director
{
    /// <summary>
    /// Plain-class glue for the dilemma flow: asks the gate and the picker for each car, opens a dilemma when the
    /// boosted car actually arrives, and resolves it when the blocked station frees or that car is parked anywhere.
    /// Publishes DilemmaCreated, DilemmaResolved and ReturnForwardWanted, and keeps the counters in DirectorState.
    /// </summary>
    public sealed class DilemmaDirector : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly ArchetypePicker _picker;
        private readonly DilemmaGate _gate;
        private readonly IReadOnlyList<IDilemmaRule> _rules;
        private readonly DirectorState _state;
        private bool _hasPending;
        private DilemmaKind _pendingRule;

        public DilemmaDirector(IEventBus bus, IGameClock clock, ArchetypePicker picker, DilemmaGate gate,
            IReadOnlyList<IDilemmaRule> rules, DirectorState state)
        {
            _bus = bus; _clock = clock; _picker = picker; _gate = gate; _rules = rules; _state = state;
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<CarSpawned>(OnCarSpawned);
            _bus.Subscribe<CarParked>(OnCarParked);
            _bus.Subscribe<CarEnteredStation>(OnCarEnteredStation);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<CarSpawned>(OnCarSpawned);
            _bus.Unsubscribe<CarParked>(OnCarParked);
            _bus.Unsubscribe<CarEnteredStation>(OnCarEnteredStation);
        }

        /// <summary>Picks the next car. Must be called before PatronScheduled is published, so the spawn finds the pick pending.</summary>
        public PickResult Choose(in LotSnapshot lot, in DilemmaContext context)
        {
            var allowed = _gate.CanFire(lot, context.Gap, context.Rising, context.Relief);
            var pick = _picker.Pick(new PickRequest(lot, context.Weights, context.Band, context.Relief, allowed, context.Budget));
            _hasPending = pick.HasRule;
            _pendingRule = pick.Rule;
            _state.ReturnForwardWanted = pick.WantsEarlyReturn;
            if (pick.WantsEarlyReturn) _bus.Publish(new ReturnForwardWanted());
            return pick;
        }

        /// <summary>Once per director tick: resolves the open dilemma when its trigger no longer holds, and updates the view.</summary>
        public void Tick(in LotSnapshot lot)
        {
            if (_gate.IsOpen)
            {
                var rule = DilemmaRules.Find(_rules, _gate.OpenKind);
                if (rule == null || !rule.Evaluate(lot).HasValue) Resolve(lot.NowSeconds);
            }
            _state.HasOpenDilemma = _gate.IsOpen;
            _state.OpenDilemma = _gate.OpenKind;
            _state.DilemmaCooldownLeft = _gate.CooldownLeft(lot.NowSeconds);
        }

        private float Now() => _clock.ShiftLengthSeconds - _clock.SecondsRemaining;

        private void OnShiftStarted(ShiftStarted e)
        {
            _hasPending = false;
            _gate.Reset();
            _picker.Reset();
        }

        private void OnCarSpawned(CarSpawned e)
        {
            if (!_hasPending) return;
            _hasPending = false;
            _gate.Open(_pendingRule, e.Car, Now());
            _state.CountMade();
            _state.HasOpenDilemma = true;
            _state.OpenDilemma = _pendingRule;
            _bus.Publish(new DilemmaCreated(_pendingRule, e.Car));
        }

        private void OnCarParked(CarParked e) => ResolveIfOpenCar(e.Car);

        private void OnCarEnteredStation(CarEnteredStation e) => ResolveIfOpenCar(e.Car);

        private void ResolveIfOpenCar(CarId car)
        {
            if (_gate.IsOpen && car == _gate.OpenCar) Resolve(Now());
        }

        private void Resolve(float now)
        {
            if (!_gate.TryResolve(now, out var kind, out var seconds)) return;
            _state.CountResolved(seconds);
            _state.HasOpenDilemma = false;
            _state.DilemmaCooldownLeft = _gate.CooldownLeft(now);
            _bus.Publish(new DilemmaResolved(kind, seconds));
            _state.Log(new DirectorDecision(DecisionKind.DilemmaResolved, now, ScheduleReason.OnTarget, default,
                CarArchetype.Plain, false, _state.ArrivalsUsed, _state.Workload, _state.Target, true, kind, 1f, seconds));
        }
    }
}
