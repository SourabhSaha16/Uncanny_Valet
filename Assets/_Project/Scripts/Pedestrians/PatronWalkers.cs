using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// The patrons on foot, from a pool of people. A patron gets out of the arrived car and walks to the diner, comes
    /// out again when summoned and waits at the curb (announcing PatronReachedCurb), then boards the delivered car
    /// (PatronBoarded) or vanishes quietly if the ticket ends without a delivery. One person per car at most.
    /// </summary>
    public sealed class PatronWalkers : IPatronLocator
    {
        private const int LookStride = 7;
        private const int LookOffset = 3;

        private readonly IEventBus _bus;
        private readonly IPedestrianPaths _paths;
        private readonly ICarFactory _cars;
        private readonly WalkerRouter _router;
        private readonly PedestrianConfig _config;
        private readonly Stack<PedestrianWalker> _free = new Stack<PedestrianWalker>();
        private readonly Dictionary<CarId, PedestrianWalker> _byCar = new Dictionary<CarId, PedestrianWalker>();
        private readonly List<PedestrianWalker> _active = new List<PedestrianWalker>();
        private readonly bool[] _slotTaken;

        public PatronWalkers(IEventBus bus, IPedestrianPaths paths, ICarFactory cars, WalkerRouter router,
            PedestrianConfig config, List<PedestrianWalker> pool)
        {
            _bus = bus; _paths = paths; _cars = cars; _router = router; _config = config;
            _slotTaken = new bool[Mathf.Max(1, paths.CurbSlotCount)];
            foreach (var walker in pool) _free.Push(walker);
            _bus.Subscribe<CarArrived>(OnCarArrived);
            _bus.Subscribe<PatronSummoned>(OnSummoned);
            _bus.Subscribe<CarDelivered>(OnDelivered);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<CarArrived>(OnCarArrived);
            _bus.Unsubscribe<PatronSummoned>(OnSummoned);
            _bus.Unsubscribe<CarDelivered>(OnDelivered);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
        }

        public bool TryGetWaitingPatron(CarId car, out Vector2 position)
        {
            var found = _byCar.TryGetValue(car, out var walker) && walker.Brain.Phase == PedestrianPhase.Waiting;
            position = found ? walker.Position : Vector2.zero;
            return found;
        }

        /// <summary>Applies phase changes the events caused and releases people who have vanished. Call every frame.</summary>
        public void Sync()
        {
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var walker = _active[i];
                if (walker.Brain.Phase != walker.Applied) _router.Apply(walker);
                if (walker.Brain.Phase == PedestrianPhase.Idle) Release(walker, i);
            }
        }

        /// <summary>A patron's walk ended: move on, and tell the game when they reach the curb.</summary>
        public void OnRouteFinished(PedestrianWalker walker)
        {
            walker.Brain.ReachedRouteEnd();
            _router.Apply(walker);
            if (walker.Brain.Phase != PedestrianPhase.Waiting) return;
            walker.FaceToward(_paths.CurbCenter);
            _bus.Publish(new PatronReachedCurb(walker.Car));
        }

        private void OnCarArrived(CarArrived e)
        {
            if (_byCar.ContainsKey(e.Car) || !_cars.TryGet(e.Car, out var car) || car.Transform == null) return;
            var walker = Rent(e.Car);
            if (walker == null) return;
            walker.Brain.BeginPatron();
            walker.PlaceAt(car.Transform.position);
            walker.SetVisible(true);
            _router.Apply(walker);
        }

        private void OnSummoned(PatronSummoned e)
        {
            if (!_byCar.TryGetValue(e.Car, out var walker))
            {
                walker = Rent(e.Car);
                if (walker == null) return;
                walker.Brain.BeginPatronInside();
                walker.PlaceAt(_paths.DinerDoor);
                walker.SetVisible(false);
                walker.Applied = PedestrianPhase.Inside;
            }
            if (walker.Slot < 0) walker.Slot = TakeSlot();
            walker.Brain.Summon();
        }

        private void OnDelivered(CarDelivered e)
        {
            if (_byCar.TryGetValue(e.Car, out var walker)) walker.Delivered = true;
        }

        private void OnClosed(TicketClosed e)
        {
            if (_byCar.TryGetValue(e.Car, out var walker)) walker.Brain.Dismiss(walker.Delivered);
        }

        private void OnShiftEnded(ShiftEnded e)
        {
            foreach (var walker in _active) walker.Brain.Dismiss(false);
        }

        private PedestrianWalker Rent(CarId car)
        {
            if (_free.Count == 0) return null;
            var walker = _free.Pop();
            walker.Car = car;
            walker.Delivered = false;
            walker.Slot = -1;
            var look = PedestrianPlan.Look(car.Value * LookStride + LookOffset, _config.Skins, _config.Hairs, _config.Outfits);
            walker.Dress(look, _config.PatronSpeed);
            _byCar[car] = walker;
            _active.Add(walker);
            return walker;
        }

        private void Release(PedestrianWalker walker, int activeIndex)
        {
            if (walker.Delivered) _bus.Publish(new PatronBoarded(walker.Car)); // the delivered car may drive off now
            if (walker.Slot >= 0) _slotTaken[walker.Slot] = false;
            walker.Slot = -1;
            walker.SetVisible(false);
            _byCar.Remove(walker.Car);
            _active.RemoveAt(activeIndex);
            _free.Push(walker);
        }

        private int TakeSlot()
        {
            for (var i = 0; i < _slotTaken.Length; i++)
            {
                if (_slotTaken[i]) continue;
                _slotTaken[i] = true;
                return i;
            }
            return 0;
        }
    }
}
