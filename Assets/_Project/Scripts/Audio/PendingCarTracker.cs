using System;
using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Audio
{
    /// <summary>
    /// Counts cars waiting on the valet from events: dropped off and not yet parked, plus pickups requested
    /// and not yet handed back. Resets when a night starts or ends.
    /// </summary>
    public sealed class PendingCarTracker
    {
        private readonly IEventBus _bus;
        private readonly HashSet<CarId> _awaitingPark = new HashSet<CarId>();
        private readonly HashSet<CarId> _awaitingPickup = new HashSet<CarId>();
        private bool _active;

        public PendingCarTracker(IEventBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <summary>Cars awaiting parking plus cars awaiting hand-back.</summary>
        public int Count => _awaitingPark.Count + _awaitingPickup.Count;

        public void Subscribe()
        {
            if (_active) return;
            _active = true;
            _bus.Subscribe<TicketOpened>(OnTicketOpened);
            _bus.Subscribe<CarParked>(OnCarParked);
            _bus.Subscribe<PickupRequested>(OnPickupRequested);
            _bus.Subscribe<TicketClosed>(OnTicketClosed);
            _bus.Subscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Subscribe<NightStarted>(OnNightStarted);
            _bus.Subscribe<NightEnded>(OnNightEnded);
        }

        public void Unsubscribe()
        {
            if (!_active) return;
            _active = false;
            _bus.Unsubscribe<TicketOpened>(OnTicketOpened);
            _bus.Unsubscribe<CarParked>(OnCarParked);
            _bus.Unsubscribe<PickupRequested>(OnPickupRequested);
            _bus.Unsubscribe<TicketClosed>(OnTicketClosed);
            _bus.Unsubscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Unsubscribe<NightStarted>(OnNightStarted);
            _bus.Unsubscribe<NightEnded>(OnNightEnded);
        }

        private void OnTicketOpened(TicketOpened e) => _awaitingPark.Add(e.Car);
        private void OnCarParked(CarParked e) => _awaitingPark.Remove(e.Car);
        private void OnPickupRequested(PickupRequested e) => _awaitingPickup.Add(e.Car);

        private void OnTicketClosed(TicketClosed e)
        {
            _awaitingPark.Remove(e.Car);
            _awaitingPickup.Remove(e.Car);
        }

        private void OnTimerExpired(TicketTimerExpired e)
        {
            if (e.Kind == TimerKind.Park) _awaitingPark.Remove(e.Car);
        }

        private void OnNightStarted(NightStarted e) => Clear();
        private void OnNightEnded(NightEnded e) => Clear();

        private void Clear()
        {
            _awaitingPark.Clear();
            _awaitingPickup.Clear();
        }
    }
}
