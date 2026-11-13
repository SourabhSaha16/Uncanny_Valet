using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Director
{
    /// <summary>
    /// Rule system that listens to the existing car, ticket and station events and owns the lot snapshot. It is the
    /// only writer of that state; everyone else calls <see cref="Capture"/>. Station slot states are derived, so the
    /// Reserved state is a guess: a car that needs the station is being driven somewhere.
    /// </summary>
    public sealed class LotStateTracker : ILotSnapshotSource, IDisposable
    {
        private const float AngryRetentionSeconds = 120f;

        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly ITuning _tuning;
        private readonly CarBook _book;
        private readonly SnapshotBuilder _builder = new SnapshotBuilder();
        private readonly List<float> _angry = new List<float>();

        /// <param name="regularSpots">Regular (non-charging) parking spots in the lot.</param>
        /// <param name="chargingBays">EV charging bays in the lot.</param>
        public LotStateTracker(IEventBus bus, IGameClock clock, ITuning tuning, IModifierCatalog catalog, int regularSpots, int chargingBays)
        {
            _bus = bus; _clock = clock; _tuning = tuning;
            _book = new CarBook(catalog, regularSpots, chargingBays);
            Subscribe();
        }

        /// <inheritdoc />
        public LotSnapshot Capture()
        {
            var perMinute = _tuning.TryGet(TuningKey.GameMinutesPerSecond, out var v) && v > 0f ? v : 1f;
            return _builder.Build(_book, _angry, NowSeconds(), _clock.ElapsedMinutes, 1f / perMinute);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
            _bus.Unsubscribe<ShiftPlanChanged>(OnPlan);
            _bus.Unsubscribe<CarSpawned>(OnSpawned);
            _bus.Unsubscribe<TicketOpened>(OnOpened);
            _bus.Unsubscribe<TicketModifiersAssigned>(OnModifiers);
            _bus.Unsubscribe<CarArrived>(OnArrived);
            _bus.Unsubscribe<StayStarted>(OnStayStarted);
            _bus.Unsubscribe<CarEntered>(OnEntered);
            _bus.Unsubscribe<CarExited>(OnExited);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Unsubscribe<CarEnteredStation>(OnStationEntered);
            _bus.Unsubscribe<CarLeftStation>(OnStationLeft);
            _bus.Unsubscribe<ModifierProgressChanged>(OnProgress);
            _bus.Unsubscribe<ModifierCleared>(OnCleared);
            _bus.Unsubscribe<PickupRequested>(OnPickup);
            _bus.Unsubscribe<TicketTimerChanged>(OnTimer);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
            _bus.Unsubscribe<CurseApplied>(OnCurse);
            _bus.Unsubscribe<PatronReturnShifted>(OnReturnShifted);
        }

        private void Subscribe()
        {
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
            _bus.Subscribe<ShiftPlanChanged>(OnPlan);
            _bus.Subscribe<CarSpawned>(OnSpawned);
            _bus.Subscribe<TicketOpened>(OnOpened);
            _bus.Subscribe<TicketModifiersAssigned>(OnModifiers);
            _bus.Subscribe<CarArrived>(OnArrived);
            _bus.Subscribe<StayStarted>(OnStayStarted);
            _bus.Subscribe<CarEntered>(OnEntered);
            _bus.Subscribe<CarExited>(OnExited);
            _bus.Subscribe<CarParked>(OnParked);
            _bus.Subscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Subscribe<CarEnteredStation>(OnStationEntered);
            _bus.Subscribe<CarLeftStation>(OnStationLeft);
            _bus.Subscribe<ModifierProgressChanged>(OnProgress);
            _bus.Subscribe<ModifierCleared>(OnCleared);
            _bus.Subscribe<PickupRequested>(OnPickup);
            _bus.Subscribe<TicketTimerChanged>(OnTimer);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<CurseApplied>(OnCurse);
            _bus.Subscribe<PatronReturnShifted>(OnReturnShifted);
        }

        private float NowSeconds() => _clock.ShiftLengthSeconds - _clock.SecondsRemaining;

        private void Reset()
        {
            _book.Clear();
            _angry.Clear();
        }

        private void OnShiftStarted(ShiftStarted e) => Reset();
        private void OnShiftEnded(ShiftEnded e) => Reset();
        private void OnPlan(ShiftPlanChanged e) => _book.ApplyPlan(e.Plan.ActiveSpots, e.Plan.ChargingOpen, e.Plan.WashOpen);
        private void OnSpawned(CarSpawned e) => _book.Spawned(e.Car);
        private void OnOpened(TicketOpened e) => _book.Stay(e.Car, e.StayMinutes);
        private void OnModifiers(TicketModifiersAssigned e) => _book.Modifiers(e.Car, e.Modifiers);
        private void OnArrived(CarArrived e) => _book.Arrived(e.Car);
        private void OnStayStarted(StayStarted e) => _book.StayStarted(e.Car, e.Minute);
        private void OnEntered(CarEntered e) => _book.Driven(e.Car, true);
        private void OnExited(CarExited e) => _book.Driven(e.Car, false);
        private void OnParked(CarParked e) => _book.Parked(e.Car, e.SpotId);
        private void OnLeftSpot(CarLeftSpot e) => _book.LeftSpot(e.Car, e.SpotId);
        private void OnStationEntered(CarEnteredStation e) => _book.StationEntered(e.Car, e.Station);
        private void OnStationLeft(CarLeftStation e) => _book.StationLeft(e.Car, e.Station);
        private void OnProgress(ModifierProgressChanged e) => _book.Progress(e.Car, e.Modifier, e.Progress01);
        private void OnCleared(ModifierCleared e) => _book.Cleared(e.Car, e.Modifier);
        private void OnPickup(PickupRequested e) => _book.PickupRequested(e.Car);
        private void OnTimer(TicketTimerChanged e) => _book.TimerChanged(e.Car, e.Elapsed01);
        private void OnClosed(TicketClosed e) => _book.Closed(e.Car);
        private void OnReturnShifted(PatronReturnShifted e) => _book.ReturnShifted(e.Car, e.DeltaMinutes);

        private void OnCurse(CurseApplied e)
        {
            var now = NowSeconds();
            while (_angry.Count > 0 && now - _angry[0] > AngryRetentionSeconds) _angry.RemoveAt(0);
            _angry.Add(now);
        }
    }
}
