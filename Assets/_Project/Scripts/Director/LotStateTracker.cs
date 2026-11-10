using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

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
        public LotStateTracker(IEventBus bus, IGameClock clock, ITuning tuning, int regularSpots, int chargingBays)
        {
            _bus = bus; _clock = clock; _tuning = tuning;
            _book = new CarBook(regularSpots, chargingBays);
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
            _bus.Unsubscribe<TicketTraitsAssigned>(OnTraits);
            _bus.Unsubscribe<CarArrived>(OnArrived);
            _bus.Unsubscribe<CarEntered>(OnEntered);
            _bus.Unsubscribe<CarExited>(OnExited);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Unsubscribe<CarEnteredStation>(OnStationEntered);
            _bus.Unsubscribe<CarLeftStation>(OnStationLeft);
            _bus.Unsubscribe<CarCleanChanged>(OnCleanChanged);
            _bus.Unsubscribe<CarCleaned>(OnCleaned);
            _bus.Unsubscribe<CarFullyCharged>(OnCharged);
            _bus.Unsubscribe<PickupRequested>(OnPickup);
            _bus.Unsubscribe<TicketTimerChanged>(OnTimer);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
            _bus.Unsubscribe<ComplaintFiled>(OnComplaint);
        }

        private void Subscribe()
        {
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
            _bus.Subscribe<ShiftPlanChanged>(OnPlan);
            _bus.Subscribe<CarSpawned>(OnSpawned);
            _bus.Subscribe<TicketOpened>(OnOpened);
            _bus.Subscribe<TicketTraitsAssigned>(OnTraits);
            _bus.Subscribe<CarArrived>(OnArrived);
            _bus.Subscribe<CarEntered>(OnEntered);
            _bus.Subscribe<CarExited>(OnExited);
            _bus.Subscribe<CarParked>(OnParked);
            _bus.Subscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Subscribe<CarEnteredStation>(OnStationEntered);
            _bus.Subscribe<CarLeftStation>(OnStationLeft);
            _bus.Subscribe<CarCleanChanged>(OnCleanChanged);
            _bus.Subscribe<CarCleaned>(OnCleaned);
            _bus.Subscribe<CarFullyCharged>(OnCharged);
            _bus.Subscribe<PickupRequested>(OnPickup);
            _bus.Subscribe<TicketTimerChanged>(OnTimer);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<ComplaintFiled>(OnComplaint);
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
        private void OnTraits(TicketTraitsAssigned e) => _book.Traits(e.Car, e.IsEv, e.IsDirty);
        private void OnArrived(CarArrived e) => _book.Arrived(e.Car);
        private void OnEntered(CarEntered e) => _book.Driven(e.Car, true);
        private void OnExited(CarExited e) => _book.Driven(e.Car, false);
        private void OnParked(CarParked e) => _book.Parked(e.Car, e.SpotId, _clock.ElapsedMinutes);
        private void OnLeftSpot(CarLeftSpot e) => _book.LeftSpot(e.Car, e.SpotId);
        private void OnStationEntered(CarEnteredStation e) => _book.StationEntered(e.Car, e.Station);
        private void OnStationLeft(CarLeftStation e) => _book.StationLeft(e.Car, e.Station);
        private void OnCleanChanged(CarCleanChanged e) => _book.CleanChanged(e.Car, e.Clean01);
        private void OnCleaned(CarCleaned e) => _book.Cleaned(e.Car);
        private void OnCharged(CarFullyCharged e) => _book.FullyCharged(e.Car);
        private void OnPickup(PickupRequested e) => _book.PickupRequested(e.Car);
        private void OnTimer(TicketTimerChanged e) => _book.TimerChanged(e.Car, e.Elapsed01);
        private void OnClosed(TicketClosed e) => _book.Closed(e.Car);

        private void OnComplaint(ComplaintFiled e)
        {
            var now = NowSeconds();
            while (_angry.Count > 0 && now - _angry[0] > AngryRetentionSeconds) _angry.RemoveAt(0);
            _angry.Add(now);
        }
    }
}
