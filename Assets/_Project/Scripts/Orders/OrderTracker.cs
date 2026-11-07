using System;
using UncannyValet.Core.Events;

namespace UncannyValet.Orders
{
    /// <summary>
    /// Watches the lot: which cars are parked and showing a letter, and every spot each car visited.
    /// Everything about a car is dropped when its ticket closes, and everything is dropped at night end.
    /// </summary>
    public sealed class OrderTracker : IEventSubscriber
    {
        private readonly IEventBus _bus;
        private readonly ParkedCars _parked;
        private readonly CarVisitHistory _history;
        private float _minute;

        public OrderTracker(IEventBus bus, ParkedCars parked, CarVisitHistory history)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _parked = parked ?? throw new ArgumentNullException(nameof(parked));
            _history = history ?? throw new ArgumentNullException(nameof(history));
        }

        public void Subscribe()
        {
            _bus.Subscribe<NightStarted>(OnNightStarted);
            _bus.Subscribe<NightTick>(OnNightTick);
            _bus.Subscribe<NightEnded>(OnNightEnded);
            _bus.Subscribe<CarLetterAssigned>(OnLetter);
            _bus.Subscribe<CarParked>(OnParked);
            _bus.Subscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Subscribe<PickupRequested>(OnPickupRequested);
            _bus.Subscribe<TicketClosed>(OnTicketClosed);
        }

        public void Unsubscribe()
        {
            _bus.Unsubscribe<NightStarted>(OnNightStarted);
            _bus.Unsubscribe<NightTick>(OnNightTick);
            _bus.Unsubscribe<NightEnded>(OnNightEnded);
            _bus.Unsubscribe<CarLetterAssigned>(OnLetter);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Unsubscribe<PickupRequested>(OnPickupRequested);
            _bus.Unsubscribe<TicketClosed>(OnTicketClosed);
        }

        private void OnNightStarted(NightStarted e) { Clear(); _minute = 0f; }
        private void OnNightEnded(NightEnded e) => Clear();
        private void OnNightTick(NightTick e) => _minute = e.MinuteOfNight;
        private void OnLetter(CarLetterAssigned e) => _parked.SetLetter(e.Car, e.Letter);
        private void OnLeftSpot(CarLeftSpot e) => _parked.LeaveSpot(e.Car, e.SpotId);
        private void OnPickupRequested(PickupRequested e) => _parked.RequestPickup(e.Car);

        private void OnParked(CarParked e)
        {
            _parked.Park(e.Car, e.SpotId);
            _history.RecordParked(e.Car, e.SpotId, _minute);
        }

        private void OnTicketClosed(TicketClosed e)
        {
            _parked.Forget(e.Car);
            _history.Clear(e.Car);
        }

        private void Clear()
        {
            _parked.Clear();
            _history.ClearAll();
        }
    }
}
