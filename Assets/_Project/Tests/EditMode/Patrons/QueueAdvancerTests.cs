using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;
using UnityEngine;

namespace UncannyValet.Tests.Patrons
{
    /// <summary>The valet pickup spot flow with fakes (issue 127): the patron steps out only when the valet is at the pickup car, the queue moves up.</summary>
    public class QueueAdvancerTests
    {
        private sealed class QueueCar : IDrivable
        {
            public QueueCar(CarId id, Transform transform) { Id = id; Transform = transform; }
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
            public readonly Dictionary<CarId, QueueCar> All = new Dictionary<CarId, QueueCar>();
            public readonly List<(CarId car, Vector2 to)> Drives = new List<(CarId, Vector2)>();
            public IDrivable Spawn(CarId id, Vector2 position, float rotationDegrees) => null;
            public IDrivable SpawnArriving(CarId id, Vector2 stopPosition, float rotationDegrees, int arrivalSlot) => null;
            public void DriveAwayAndDespawn(CarId id) { }
            public bool TryGet(CarId id, out IDrivable car) { var found = All.TryGetValue(id, out var c); car = c; return found; }
            public void Despawn(CarId id) { }

            public bool TryDriveTo(CarId id, Vector2 target, float speed)
            {
                if (!All.TryGetValue(id, out var car)) return false;
                Drives.Add((id, target));
                car.Transform.position = target;
                return true;
            }
        }

        private sealed class Lot : ILotLayout
        {
            public Vector2 ArrivalPoint => Slots[0];
            public float ArrivalRotationDegrees => 0f;
            public Vector2[] Slots { get; } = { new Vector2(0f, 0f), new Vector2(0f, -4.65f), new Vector2(0f, -9.3f), new Vector2(0f, -13.95f) };
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

        private sealed class FakeScan : IArrivalScan
        {
            public readonly List<bool> Occupied = new List<bool> { false, false, false, false };
            public IReadOnlyList<bool> Scan(int maxSlots) => Occupied;
        }

        private sealed class Focus : IPlayerFocus
        {
            public Vector2 Position { get; set; } = new Vector2(30f, 30f);
        }

        private EventBus _bus;
        private Cars _cars;
        private Lot _lot;
        private FakeScan _scan;
        private Focus _focus;
        private QueueAdvancer _queue;
        private readonly List<PatronStepsOut> _out = new List<PatronStepsOut>();
        private readonly List<GameObject> _objects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus(); _cars = new Cars(); _lot = new Lot(); _scan = new FakeScan(); _focus = new Focus();
            _queue = new QueueAdvancer(_bus, _cars, _lot, _scan, new PickupQueueSettings()) { Focus = _focus };
            _out.Clear();
            _bus.Subscribe<PatronStepsOut>(e => _out.Add(e));
        }

        [TearDown]
        public void TearDown()
        {
            _queue.Dispose();
            foreach (var go in _objects) Object.DestroyImmediate(go);
            _objects.Clear();
        }

        private CarId Arrive(int id, int slot)
        {
            var go = new GameObject("car " + id);
            _objects.Add(go);
            go.transform.position = _lot.Slots[slot];
            var car = new CarId(id);
            _cars.All[car] = new QueueCar(car, go.transform);
            _scan.Occupied[slot] = true;
            _bus.Publish(new CarArrived(car));
            return car;
        }

        [Test]
        public void ThePatronStaysInTheCar_WhileTheValetIsFarAway()
        {
            Arrive(1, 0);
            _queue.Tick();
            Assert.AreEqual(0, _out.Count);
        }

        [Test]
        public void ThePatronStepsOut_OnceTheValetComesUpToTheCarInThePickupSpot()
        {
            var a = Arrive(1, 0);
            _focus.Position = new Vector2(2f, 1f);
            _queue.Tick();
            _queue.Tick();
            Assert.AreEqual(1, _out.Count);
            Assert.AreEqual(a, _out[0].Car);
        }

        [Test]
        public void TheValetNextToACarFurtherBack_DoesNotLetItsPatronOut()
        {
            Arrive(1, 0);
            Arrive(2, 1);
            Arrive(3, 2);
            _focus.Position = new Vector2(0f, -9.3f);
            _queue.Tick();
            Assert.AreEqual(0, _out.Count, "the valet stands at the third car, but only the pickup spot's car lets its patron out");
        }

        [Test]
        public void WithoutAPlayerFocusYet_NothingHappensAndNothingBreaks()
        {
            _queue.Focus = null;
            Arrive(1, 0);
            Assert.DoesNotThrow(() => _queue.Tick());
            Assert.AreEqual(0, _out.Count);
        }

        [Test]
        public void WhenThePickupCarIsDrivenAway_TheCarBehindDrivesIntoThePickupSpot()
        {
            var a = Arrive(1, 0);
            var b = Arrive(2, 1);
            _bus.Publish(new CarEntered(a));
            _scan.Occupied[0] = false;
            _queue.Tick();
            Assert.AreEqual(1, _cars.Drives.Count);
            Assert.AreEqual(b, _cars.Drives[0].car);
            Assert.AreEqual(_lot.Slots[0], _cars.Drives[0].to);
        }

        [Test]
        public void TheCarBehindWaits_WhileThePickupCarStillBlocksTheSpot()
        {
            var a = Arrive(1, 0);
            Arrive(2, 1);
            _bus.Publish(new CarEntered(a));
            _queue.Tick();
            Assert.AreEqual(0, _cars.Drives.Count);
        }

        [Test]
        public void TheQueueMovesUpOneByOne_AsEachSlotClears()
        {
            var a = Arrive(1, 0);
            Arrive(2, 1);
            var c = Arrive(3, 2);
            _bus.Publish(new CarEntered(a));
            _scan.Occupied[0] = false;
            _queue.Tick();
            Assert.AreEqual(1, _cars.Drives.Count, "only the car behind the pickup spot moves first");
            _bus.Publish(new CarRepositioned(new CarId(2)));
            _scan.Occupied[1] = false;
            _scan.Occupied[0] = true;
            _queue.Tick();
            Assert.AreEqual(2, _cars.Drives.Count);
            Assert.AreEqual(c, _cars.Drives[1].car);
            Assert.AreEqual(_lot.Slots[1], _cars.Drives[1].to);
        }

        [Test]
        public void ACarTakenFromTheBackOfTheQueue_LetsItsPatronOutAtOnce()
        {
            Arrive(1, 0);
            var b = Arrive(2, 1);
            _bus.Publish(new CarEntered(b));
            Assert.AreEqual(1, _out.Count);
            Assert.AreEqual(b, _out[0].Car);
        }

        [Test]
        public void ACarWhoseTicketClosed_IsForgotten()
        {
            var a = Arrive(1, 0);
            _bus.Publish(new TicketClosed(new TicketId(1), a, 0f, 0));
            _focus.Position = Vector2.zero;
            _queue.Tick();
            Assert.AreEqual(0, _out.Count);
        }
    }
}
