using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>
    /// A lightweight fake lot with a scripted bot for the soak test: it spawns the cars the director schedules,
    /// parks every car in the nearest suitable place (the wash or a bay when it needs one and it is free, else the
    /// lowest free spot) and delivers the oldest pickup first. One worker, so tasks take real time.
    /// </summary>
    internal sealed partial class SoakSim
    {
        public const int RegularSpots = 9;
        public const int Bays = 2;
        public const int QueueCap = 4;
        public const float WashSeconds = 15f;
        public const float ChargeSeconds = 20f;
        public const float PickupWindowSeconds = 60f;
        private const float ArriveSeconds = 3f;
        private const float ParkTaskSeconds = 8f;
        private const float DeliverTaskSeconds = 10f;
        private const float StayMin = 20f;
        private const float StayMax = 45f;

        private readonly IEventBus _bus;
        private readonly Random _random;
        private readonly List<SoakCar> _cars = new List<SoakCar>();
        private int _nextId = 1;
        private int _requestCounter;
        private bool _hasPending;
        private CarArchetype _pendingArchetype;
        private SoakCar _target;
        private bool _delivering;
        private float _taskLeft;

        public SoakSim(IEventBus bus, Random random)
        {
            _bus = bus; _random = random;
            _bus.Subscribe<PatronScheduled>(OnScheduled);
        }

        public IReadOnlyList<SoakCar> Cars => _cars;
        public SoakCar[] Spots { get; } = new SoakCar[RegularSpots + 1];
        public SoakCar[] BaySlots { get; } = new SoakCar[Bays];
        public SoakCar WashSlot { get; private set; }
        public List<float> AngryTimes { get; } = new List<float>();
        public float Now { get; set; }
        public int Arrivals { get; private set; }
        public int Delivered { get; private set; }

        public bool HasOpenTask
        {
            get
            {
                for (var i = 0; i < _cars.Count; i++) if (_cars[i].OpenTask) return true;
                return false;
            }
        }

        public int AwaitingCount
        {
            get
            {
                var n = 0;
                for (var i = 0; i < _cars.Count; i++) if (_cars[i].AwaitingPark) n++;
                return n;
            }
        }

        public void Dispose() => _bus.Unsubscribe<PatronScheduled>(OnScheduled);

        /// <summary>Advances the lot by dt seconds: late spawns, the cars' own clocks, then the bot.</summary>
        public void Step(float dt)
        {
            TrySpawnPending();
            for (var i = _cars.Count - 1; i >= 0; i--) Advance(_cars[i], dt);
            if (_target == null) StartTask();
            if (_target == null) return;
            _taskLeft -= dt;
            if (_taskLeft <= 0f) FinishTask();
        }

        private void OnScheduled(PatronScheduled e)
        {
            _hasPending = true;
            _pendingArchetype = e.Archetype;
            TrySpawnPending();
        }

        private void TrySpawnPending()
        {
            if (!_hasPending || AwaitingCount >= QueueCap) return;
            _hasPending = false;
            var id = new CarId(_nextId++);
            var car = new SoakCar
            {
                Id = id, Dirty = ArchetypeKinds.NeedsWash(_pendingArchetype), Ev = ArchetypeKinds.NeedsCharge(_pendingArchetype),
                State = CarLotState.Arriving, ArriveLeft = ArriveSeconds,
                StayLeft = StayMin + (StayMax - StayMin) * (float)_random.NextDouble(),
            };
            _cars.Add(car);
            Arrivals++;
            _bus.Publish(new CarSpawned(id, new TicketId(id.Value)));
        }

        private void Advance(SoakCar car, float dt)
        {
            switch (car.State)
            {
                case CarLotState.Arriving:
                    car.ArriveLeft -= dt;
                    if (car.ArriveLeft <= 0f) car.State = CarLotState.WaitingToPark;
                    break;
                case CarLotState.Parked:
                case CarLotState.InStation:
                    Serve(car, dt);
                    car.StayLeft -= dt;
                    if (car.StayLeft <= 0f) RequestPickup(car);
                    break;
                case CarLotState.PickupRequested:
                    car.PickupWaited += dt;
                    if (car.PickupWaited >= PickupWindowSeconds && car != _target) GoAngry(car);
                    break;
            }
        }

        private static void Serve(SoakCar car, float dt)
        {
            if (car.InWash && car.NeedsWash && (car.WashLeft -= dt) <= 0f) car.WashDone = true;
            if (car.Bay >= 0 && car.NeedsCharge && (car.ChargeLeft -= dt) <= 0f) car.ChargeDone = true;
        }

        private void RequestPickup(SoakCar car)
        {
            car.State = CarLotState.PickupRequested;
            car.RequestOrder = ++_requestCounter;
        }

        private void GoAngry(SoakCar car)
        {
            AngryTimes.Add(Now);
            Remove(car);
        }

        private void StartTask()
        {
            SoakCar pickup = null;
            SoakCar waiting = null;
            foreach (var car in _cars)
            {
                if (car.State == CarLotState.PickupRequested && (pickup == null || car.RequestOrder < pickup.RequestOrder)) pickup = car;
                if (car.State == CarLotState.WaitingToPark && waiting == null && DestinationOf(car).HasValue) waiting = car;
            }
            _delivering = pickup != null;
            _target = pickup ?? waiting;
            if (_target == null) return;
            _taskLeft = _delivering ? DeliverTaskSeconds : ParkTaskSeconds;
            if (!_delivering) _target.State = CarLotState.Driven;
        }

        private void FinishTask()
        {
            var car = _target;
            _target = null;
            if (_delivering) { Remove(car); Delivered++; return; }
            Place(car);
        }

        private void Remove(SoakCar car)
        {
            if (car.Spot > 0) Spots[car.Spot] = null;
            if (car.InWash) WashSlot = null;
            if (car.Bay >= 0) BaySlots[car.Bay] = null;
            _cars.Remove(car);
        }
    }
}
