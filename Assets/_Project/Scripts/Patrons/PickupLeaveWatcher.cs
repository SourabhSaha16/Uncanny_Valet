using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Publishes <see cref="CarLeftPickup"/> (issue 139) once the car the valet took has driven out of the pickup spot (slot 0).
    /// A car taken from anywhere else counts as left at once. Call <see cref="Tick"/> every frame; it never allocates.
    /// </summary>
    public sealed class PickupLeaveWatcher : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ICarFactory _cars;
        private readonly ILotLayout _lot;
        private readonly float _tolerance;
        private readonly List<CarId> _waiting = new List<CarId>();

        public PickupLeaveWatcher(IEventBus bus, ICarFactory cars, ILotLayout lot, float slotTolerance)
        {
            _bus = bus; _cars = cars; _lot = lot; _tolerance = slotTolerance;
            _bus.Subscribe<CarEntered>(OnEntered);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<ShiftStarted>(OnShiftEdge);
            _bus.Subscribe<ShiftEnded>(OnShiftEdge);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<CarEntered>(OnEntered);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
            _bus.Unsubscribe<ShiftStarted>(OnShiftEdge);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEdge);
        }

        /// <summary>Releases each watched car as soon as it is no longer in the pickup spot.</summary>
        public void Tick()
        {
            for (var i = _waiting.Count - 1; i >= 0; i--)
            {
                if (InPickupSpot(_waiting[i])) continue;
                var car = _waiting[i];
                _waiting.RemoveAt(i);
                _bus.Publish(new CarLeftPickup(car));
            }
        }

        private bool InPickupSpot(CarId car)
        {
            if (!_cars.TryGet(car, out var drivable) || drivable.Transform == null) return false;
            return QueueSlotLocator.SlotAt(_lot.ArrivalSlots, drivable.Transform.position, _tolerance) == QueueAdvanceRule.PickupSlot;
        }

        private void OnEntered(CarEntered e)
        {
            if (_waiting.Contains(e.Car)) return;
            if (InPickupSpot(e.Car)) _waiting.Add(e.Car);
            else _bus.Publish(new CarLeftPickup(e.Car));
        }

        private void OnClosed(TicketClosed e) => _waiting.Remove(e.Car);

        private void OnShiftEdge<T>(T e) => _waiting.Clear();
    }
}
