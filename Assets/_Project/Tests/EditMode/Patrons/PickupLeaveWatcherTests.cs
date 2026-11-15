using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;
using UnityEngine;

namespace UncannyValet.Tests.Patrons
{
    /// <summary>CarLeftPickup is published once the valet's car is out of slot 0, or at once for a car taken elsewhere (issue 139).</summary>
    public class PickupLeaveWatcherTests
    {
        private const float Tolerance = 1.5f;

        private sealed class WatchedCar : IDrivable
        {
            public WatchedCar(CarId id, Transform transform) { Id = id; Transform = transform; }
            public CarId Id { get; }
            public Transform Transform { get; }
            public float Speed => 0f;
            public bool HasDriver => false;
            public void SetInput(float throttle, float steer) { }
            public void SetDrift(bool drift) { }
            public void SetDriver(bool hasDriver) { }
        }

        private sealed class Cars : ICarFactory
        {
            public readonly Dictionary<CarId, WatchedCar> All = new Dictionary<CarId, WatchedCar>();
            public IDrivable Spawn(CarId id, Vector2 position, float rotationDegrees) => null;
            public IDrivable SpawnArriving(CarId id, Vector2 stopPosition, float rotationDegrees, int arrivalSlot) => null;
            public void DriveAwayAndDespawn(CarId id) { }
            public bool TryGet(CarId id, out IDrivable car) { var found = All.TryGetValue(id, out var c); car = c; return found; }
            public void Despawn(CarId id) { }
            public bool TryDriveTo(CarId id, Vector2 target, float speed) => false;
        }

        private sealed class Lot : ILotLayout
        {
            public Vector2 ArrivalPoint => Slots[0];
            public float ArrivalRotationDegrees => 0f;
            public Vector2[] Slots { get; } = { new Vector2(0f, 0f), new Vector2(0f, -4.65f), new Vector2(0f, -9.3f) };
            public IReadOnlyList<Vector2> ArrivalSlots => Slots;
            public Vector2 ValetStartPoint => Vector2.zero;
            public IReadOnlyList<int> SpotIds => new int[0];
            public int RegularSpotCount => 0;
            public int ChargingSpotCount => 0;
            public bool HasWashStation => false;
            public int WashStationCount => 0;
            public bool IsSpotFree(int spotId) => true;
            public bool IsChargingSpot(int spotId) => false;
        }

        private EventBus _bus;
        private Cars _cars;
        private Lot _lot;
        private PickupLeaveWatcher _watcher;
        private readonly List<CarLeftPickup> _left = new List<CarLeftPickup>();
        private readonly List<GameObject> _objects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus(); _cars = new Cars(); _lot = new Lot();
            _watcher = new PickupLeaveWatcher(_bus, _cars, _lot, Tolerance);
            _left.Clear();
            _bus.Subscribe<CarLeftPickup>(e => _left.Add(e));
        }

        [TearDown]
        public void TearDown()
        {
            _watcher.Dispose();
            foreach (var go in _objects) Object.DestroyImmediate(go);
            _objects.Clear();
        }

        private Transform Place(int id, Vector2 at)
        {
            var go = new GameObject("car " + id);
            _objects.Add(go);
            go.transform.position = at;
            _cars.All[new CarId(id)] = new WatchedCar(new CarId(id), go.transform);
            return go.transform;
        }

        [Test]
        public void ACarEnteredInThePickupSpot_IsReleasedOnlyOnceItHasDrivenOut()
        {
            var car = Place(1, _lot.Slots[0]);
            _bus.Publish(new CarEntered(new CarId(1)));
            _watcher.Tick();
            Assert.AreEqual(0, _left.Count, "still in the pickup spot");
            car.position = new Vector2(0f, -2.4f);
            _watcher.Tick();
            Assert.AreEqual(1, _left.Count);
            _watcher.Tick();
            Assert.AreEqual(1, _left.Count, "published once");
        }

        [Test]
        public void ACarTakenFromAnotherSlotOrFromNowhere_CountsAsLeftAtOnce()
        {
            Place(1, _lot.Slots[1]);
            _bus.Publish(new CarEntered(new CarId(1)));
            Assert.AreEqual(1, _left.Count);
            Place(2, new Vector2(20f, 20f));
            _bus.Publish(new CarEntered(new CarId(2)));
            Assert.AreEqual(2, _left.Count);
        }

        [Test]
        public void TheTicketClosingFirst_DropsTheWatch()
        {
            var car = Place(1, _lot.Slots[0]);
            _bus.Publish(new CarEntered(new CarId(1)));
            _bus.Publish(new TicketClosed(new TicketId(1), new CarId(1), 0f, 0));
            car.position = new Vector2(0f, -5f);
            _watcher.Tick();
            Assert.AreEqual(0, _left.Count);
        }

        [Test]
        public void TheShiftEnding_DropsEveryWatch()
        {
            var car = Place(1, _lot.Slots[0]);
            _bus.Publish(new CarEntered(new CarId(1)));
            _bus.Publish(new ShiftEnded());
            car.position = new Vector2(0f, -5f);
            _watcher.Tick();
            Assert.AreEqual(0, _left.Count);
        }

        [Test]
        public void ACarDespawnedWhileWatched_IsReleasedAndDropped()
        {
            Place(1, _lot.Slots[0]);
            _bus.Publish(new CarEntered(new CarId(1)));
            _cars.All.Clear();
            _watcher.Tick();
            _watcher.Tick();
            Assert.AreEqual(1, _left.Count);
        }
    }
}
