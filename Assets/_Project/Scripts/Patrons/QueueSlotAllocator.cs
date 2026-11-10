using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Owns the arrival queue's slot choices: picks the slot for a new car and, while a car drives in, re-evaluates its
    /// slot so it moves up to the first free slot it can reach (a slot freed after the car was sent is used). It only
    /// ever moves toward the curb: a car that has passed a slot cannot go back to it.
    /// The reservation of a car is released when it arrives, when its ticket closes, and when a shift starts or ends.
    /// </summary>
    public sealed class QueueSlotAllocator : IQueueSlotSource, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly TicketRegistry _registry;
        private readonly IArrivalScan _scan;
        private readonly PatronPlan _plan;
        private readonly ArrivalReservations _reservations = new ArrivalReservations();
        private readonly QueueSlotChooser _chooser = new QueueSlotChooser();
        private readonly List<bool> _ahead = new List<bool>();

        public QueueSlotAllocator(IEventBus bus, TicketRegistry registry, IArrivalScan scan, PatronPlan plan)
        {
            _bus = bus; _registry = registry; _scan = scan; _plan = plan;
            _bus.Subscribe<CarArrived>(OnCarArrived);
            _bus.Subscribe<TicketClosed>(OnTicketClosed);
            _bus.Subscribe<ShiftStarted>(OnShiftEdge);
            _bus.Subscribe<ShiftEnded>(OnShiftEdge);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<CarArrived>(OnCarArrived);
            _bus.Unsubscribe<TicketClosed>(OnTicketClosed);
            _bus.Unsubscribe<ShiftStarted>(OnShiftEdge);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEdge);
        }

        /// <summary>Cars currently holding a slot reservation.</summary>
        public int ReservedCount => _reservations.Count;

        /// <summary>The slot for a car sent now, or <see cref="ArrivalGate.NoSlot"/> (cap reached or no reachable free slot).</summary>
        public int ChooseForSpawn()
        {
            if (_registry.AwaitingParkCount >= _plan.MaxWaitingCars) return ArrivalGate.NoSlot;
            _reservations.Prune(_registry);
            var occupied = _scan.Scan(_plan.MaxWaitingCars);
            FillAhead(occupied.Count, new CarId(int.MaxValue));
            return _chooser.Choose(occupied, _ahead, _plan.MaxWaitingCars);
        }

        /// <summary>Records that <paramref name="car"/> was sent to <paramref name="slot"/>.</summary>
        public void Reserve(CarId car, int slot) => _reservations.Reserve(car, slot);

        /// <inheritdoc />
        public int Reevaluate(int currentSlot)
        {
            _reservations.Prune(_registry);
            if (!_reservations.TryHolder(currentSlot, out var car)) return currentSlot;
            var occupied = _scan.Scan(_plan.MaxWaitingCars);
            FillAhead(occupied.Count, car);
            var target = _chooser.Choose(occupied, _ahead, _plan.MaxWaitingCars);
            if (target == ArrivalGate.NoSlot || target >= currentSlot || _reservations.IsReserved(target)) return currentSlot;
            _reservations.Move(car, target);
            return target;
        }

        private void FillAhead(int count, CarId before)
        {
            _ahead.Clear();
            for (var i = 0; i < count; i++) _ahead.Add(false);
            _reservations.MarkAhead(_ahead, before);
        }

        private void OnCarArrived(CarArrived e) => _reservations.Release(e.Car);

        private void OnTicketClosed(TicketClosed e) => _reservations.Release(e.Car);

        private void OnShiftEdge<T>(T e) => _reservations.Clear();
    }
}
