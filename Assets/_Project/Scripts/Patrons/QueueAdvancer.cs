using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Runs the valet pickup spot and the queue behind it (issue 127). Every arrived car stands in a queue slot (slot 0 is the
    /// pickup spot). When the valet comes up to the car in the pickup spot its patron is told to step out
    /// (<see cref="PatronStepsOut"/>); when a slot is clear the car behind drives up into it (<see cref="QueueAdvanceRule"/>).
    /// Call <see cref="Tick"/> every frame; it never allocates.
    /// </summary>
    public sealed class QueueAdvancer : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ICarFactory _cars;
        private readonly ILotLayout _lot;
        private readonly IArrivalScan _scan;
        private readonly PickupQueueSettings _settings;
        private readonly PatronExitGate _gate = new PatronExitGate();
        private readonly List<CarId> _standing = new List<CarId>();
        private readonly Dictionary<CarId, int> _moving = new Dictionary<CarId, int>();
        private readonly List<CarId> _inSlot = new List<CarId>();
        private readonly List<bool> _occupied = new List<bool>();
        private readonly List<QueueMove> _moves = new List<QueueMove>();

        public QueueAdvancer(IEventBus bus, ICarFactory cars, ILotLayout lot, IArrivalScan scan, PickupQueueSettings settings)
        {
            _bus = bus; _cars = cars; _lot = lot; _scan = scan; _settings = settings;
            _bus.Subscribe<CarArrived>(OnArrived);
            _bus.Subscribe<CarRepositioned>(OnRepositioned);
            _bus.Subscribe<CarEntered>(OnEntered);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<ShiftStarted>(OnShiftEdge);
            _bus.Subscribe<ShiftEnded>(OnShiftEdge);
        }

        /// <summary>Where the valet is (on foot or in a car). Null until the game has one; the approach check waits for it.</summary>
        public IPlayerFocus Focus { get; set; }

        public void Dispose()
        {
            _bus.Unsubscribe<CarArrived>(OnArrived);
            _bus.Unsubscribe<CarRepositioned>(OnRepositioned);
            _bus.Unsubscribe<CarEntered>(OnEntered);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
            _bus.Unsubscribe<ShiftStarted>(OnShiftEdge);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEdge);
        }

        /// <summary>Lets the patron at the pickup spot out when the valet is near, and moves the queue up where a slot is clear.</summary>
        public void Tick()
        {
            if (_standing.Count == 0) return;
            var slots = _lot.ArrivalSlots;
            FillSlotCars(slots);
            ReleaseAtPickup(slots);
            Advance(slots);
        }

        private void FillSlotCars(IReadOnlyList<Vector2> slots)
        {
            _inSlot.Clear();
            for (var i = 0; i < slots.Count; i++) _inSlot.Add(CarId.None);
            for (var i = 0; i < _standing.Count; i++)
            {
                if (_moving.ContainsKey(_standing[i]) || !_cars.TryGet(_standing[i], out var car) || car.Transform == null) continue;
                var slot = QueueSlotLocator.SlotAt(slots, car.Transform.position, _settings.SlotTolerance);
                if (slot != QueueSlotLocator.None && _inSlot[slot] == CarId.None) _inSlot[slot] = _standing[i];
            }
        }

        private void ReleaseAtPickup(IReadOnlyList<Vector2> slots)
        {
            var car = _inSlot[QueueAdvanceRule.PickupSlot];
            if (car == CarId.None || Focus == null) return;
            var distance = Vector2.Distance(Focus.Position, slots[QueueAdvanceRule.PickupSlot]);
            if (_gate.TryReleaseByApproach(car, QueueAdvanceRule.PickupSlot, distance, _settings.ApproachRange))
                _bus.Publish(new PatronStepsOut(car));
        }

        private void Advance(IReadOnlyList<Vector2> slots)
        {
            _occupied.Clear();
            var scanned = _scan.Scan(slots.Count);
            for (var i = 0; i < scanned.Count; i++) _occupied.Add(scanned[i]);
            foreach (var target in _moving.Values) if (target < _occupied.Count) _occupied[target] = true;
            _moves.Clear();
            QueueAdvanceRule.Moves(_inSlot, _occupied, _moves);
            for (var i = 0; i < _moves.Count; i++)
                if (_cars.TryDriveTo(_moves[i].Car, slots[_moves[i].ToSlot], _settings.AdvanceSpeed)) _moving[_moves[i].Car] = _moves[i].ToSlot;
        }

        private void OnArrived(CarArrived e)
        {
            if (!_standing.Contains(e.Car)) _standing.Add(e.Car);
        }

        private void OnRepositioned(CarRepositioned e) => _moving.Remove(e.Car);

        // The valet took a queued car without the pickup approach: the patron must not ride along, so it gets out now.
        private void OnEntered(CarEntered e)
        {
            if (!_standing.Remove(e.Car)) return;
            _moving.Remove(e.Car);
            if (_gate.TryReleaseByTakeover(e.Car)) _bus.Publish(new PatronStepsOut(e.Car));
        }

        private void OnClosed(TicketClosed e) => Forget(e.Car);

        private void OnShiftEdge<T>(T e)
        {
            _standing.Clear();
            _moving.Clear();
            _gate.Clear();
        }

        private void Forget(CarId car)
        {
            _standing.Remove(car);
            _moving.Remove(car);
            _gate.Forget(car);
        }
    }
}
