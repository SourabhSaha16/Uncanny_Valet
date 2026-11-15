using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// The patrons on foot, from a pool of people. A patron gets out of the car in the pickup spot when the valet comes
    /// up to it (PatronStepsOut), walks to the diner (PatronEnteredDiner at the door), comes
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
        private readonly PatronPool<PedestrianWalker> _pool;
        private readonly PendingStepOuts _pending = new PendingStepOuts();
        private readonly Dictionary<CarId, PedestrianWalker> _byCar = new Dictionary<CarId, PedestrianWalker>();
        private readonly List<PedestrianWalker> _active = new List<PedestrianWalker>();
        private readonly bool[] _slotTaken;

        public PatronWalkers(IEventBus bus, IPedestrianPaths paths, ICarFactory cars, WalkerRouter router,
            PedestrianConfig config, List<PedestrianWalker> pool)
        {
            _bus = bus; _paths = paths; _cars = cars; _router = router; _config = config;
            _slotTaken = new bool[Mathf.Max(1, paths.CurbSlotCount)];
            _pool = new PatronPool<PedestrianWalker>(pool);
            _bus.Subscribe<PatronStepsOut>(OnStepsOut);
            _bus.Subscribe<PatronSummoned>(OnSummoned);
            _bus.Subscribe<CarDelivered>(OnDelivered);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<PatronStepsOut>(OnStepsOut);
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
            ServePending();
        }

        /// <summary>A patron's walk ended: move on, and tell the game when they reach the curb.</summary>
        public void OnRouteFinished(PedestrianWalker walker)
        {
            var wasToDoor = walker.Brain.Phase == PedestrianPhase.ToDoor;
            walker.Brain.ReachedRouteEnd();
            _router.Apply(walker);
            if (wasToDoor) _bus.Publish(new PatronEnteredDiner(walker.Car));
            if (walker.Brain.Phase != PedestrianPhase.Waiting) return;
            _bus.Publish(new PatronReachedCurb(walker.Car));
        }

        private void OnStepsOut(PatronStepsOut e)
        {
            if (_byCar.ContainsKey(e.Car) || _pending.Contains(e.Car)) return;
            if (!StepOut(e.Car)) Defer(e.Car);
        }

        private bool StepOut(CarId id)
        {
            if (!_cars.TryGet(id, out var car) || car.Transform == null) return true;
            var walker = Rent(id);
            if (walker == null) return false;
            walker.Brain.BeginPatron();
            walker.PlaceAt(car.Transform.position);
            walker.SetVisible(true);
            _router.Apply(walker);
            return true;
        }

        private void Defer(CarId id)
        {
            if (_pending.Add(id))
                Debug.LogError("PatronWalkers: no free patron person for " + id + "; the step-out waits until one is released. Raise the pool floor or check the lot sizes.");
        }

        private void ServePending()
        {
            while (_pending.Count > 0 && _pool.FreeCount > 0)
            {
                var id = _pending.CarAt(0);
                var summoned = _pending.SummonedAt(0);
                _pending.Remove(id);
                StepOut(id);
                if (summoned && _byCar.TryGetValue(id, out var walker)) SummonWalker(walker);
            }
        }

        private void OnSummoned(PatronSummoned e)
        {
            if (_pending.Contains(e.Car)) { _pending.MarkSummoned(e.Car); return; }
            if (!_byCar.TryGetValue(e.Car, out var walker))
            {
                walker = Rent(e.Car);
                if (walker == null) { Defer(e.Car); _pending.MarkSummoned(e.Car); return; }
                walker.Brain.BeginPatronInside();
                walker.PlaceAt(_paths.DinerDoor);
                walker.SetVisible(false);
                walker.Applied = PedestrianPhase.Inside;
            }
            SummonWalker(walker);
        }

        private void SummonWalker(PedestrianWalker walker)
        {
            if (walker.Slot < 0) walker.Slot = TakeSlot();
            walker.Brain.Summon();
        }

        private void OnDelivered(CarDelivered e)
        {
            if (_byCar.TryGetValue(e.Car, out var walker)) walker.Delivered = true;
        }

        private void OnClosed(TicketClosed e)
        {
            _pending.Remove(e.Car);
            if (_byCar.TryGetValue(e.Car, out var walker)) walker.Brain.Dismiss(walker.Delivered);
        }

        private void OnShiftEnded(ShiftEnded e)
        {
            _pending.Clear();
            foreach (var walker in _active) walker.Brain.Dismiss(false);
        }

        private PedestrianWalker Rent(CarId car)
        {
            if (!_pool.TryRent(out var walker)) return null;
            walker.Car = car;
            walker.Delivered = false;
            walker.Slot = -1;
            var look = PedestrianPlan.Look(car.Value * LookStride + LookOffset, _config.Skins, _config.Hairs, _config.Outfits);
            walker.Dress(look, _config.PatronSpeed);
            if (_cars.TryGet(car, out var own) && own.Transform != null) walker.BindCar(own.Transform);
            _byCar[car] = walker;
            _active.Add(walker);
            return walker;
        }

        private void Release(PedestrianWalker walker, int activeIndex)
        {
            if (walker.Delivered) _bus.Publish(new PatronBoarded(walker.Car)); // the delivered car may drive off now
            if (walker.Slot >= 0) _slotTaken[walker.Slot] = false;
            walker.Slot = -1;
            walker.ReleaseCar();
            walker.SetVisible(false);
            _byCar.Remove(walker.Car);
            _active.RemoveAt(activeIndex);
            _pool.Return(walker);
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
