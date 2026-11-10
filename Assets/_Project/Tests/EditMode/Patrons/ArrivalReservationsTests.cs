using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;
using UnityEngine;

namespace UncannyValet.Tests.Patrons
{
    public class ArrivalReservationsTests
    {
        private sealed class FakeClock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeTips : ITipCalculator
        {
            public int Calculate(in TipInput input) => input.BaseTip;
            public TipBreakdown Explain(in TipInput input) =>
                new TipBreakdown(input.BaseTip, 0, 0, 0, input.BaseTip, TipZeroReason.None);
        }

        private sealed class RecordingCars : ICarFactory
        {
            public int DriveAways, Despawns;
            public IDrivable Spawn(CarId id, Vector2 position, float rotationDegrees) => null;
            public IDrivable SpawnArriving(CarId id, Vector2 stopPosition, float rotationDegrees, int arrivalSlot) => null;
            public void DriveAwayAndDespawn(CarId id) => DriveAways++;
            public bool TryGet(CarId id, out IDrivable car) { car = null; return false; }
            public void Despawn(CarId id) => Despawns++;
        }

        private static readonly bool[] Clear = { false, false, false };
        private EventBus _bus;
        private TicketRegistry _registry;
        private ArrivalReservations _reservations;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _registry = new TicketRegistry(_bus, new FakeClock(), new FakeTips());
            _reservations = new ArrivalReservations();
        }

        [Test]
        public void ArrivingCar_KeepsItsSlotBlockedEvenWhenScanSeesNothing()
        {
            var t = _registry.Open(new CarId(1), new PatronRequest(30f, 20, false));
            _reservations.Reserve(t.TicketId, 0);
            var blocked = _reservations.Merge(Clear, _registry);
            Assert.AreEqual(1, ArrivalGate.FirstFreeSlot(_registry.AwaitingParkCount, 3, blocked));
        }

        [Test]
        public void ParkedCar_ReleasesItsSlot()
        {
            var t = _registry.Open(new CarId(1), new PatronRequest(30f, 20, false));
            _reservations.Reserve(t.TicketId, 0);
            _bus.Publish(new CarParked(t.CarId, 1));
            Assert.IsFalse(_reservations.Merge(Clear, _registry)[0]);
        }

        [Test]
        public void Departure_DrivesAwayInsteadOfDespawning()
        {
            var cars = new RecordingCars();
            var departure = new PatronDeparture(_bus, cars);
            var departed = 0;
            _bus.Subscribe<CarDeparted>(e => departed++);
            _bus.Publish(new TicketClosed(new TicketId(1), new CarId(1), 0f, 10));
            Assert.AreEqual(1, cars.DriveAways);
            Assert.AreEqual(0, cars.Despawns);
            Assert.AreEqual(1, departed);
            departure.Dispose();
        }
    }
}
