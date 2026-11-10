using System.Collections.Generic;
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

        private static List<bool> Flags(int n)
        {
            var flags = new List<bool>();
            for (var i = 0; i < n; i++) flags.Add(false);
            return flags;
        }

        [Test]
        public void ReservedSlot_IsFlaggedForCarsBehindOnly()
        {
            _reservations.Reserve(new CarId(1), 0);
            _reservations.Reserve(new CarId(2), 1);
            var flags = Flags(3);
            _reservations.MarkAhead(flags, new CarId(2));
            Assert.AreEqual(new[] { true, false, false }, flags.ToArray());
        }

        [Test]
        public void Release_FreesTheSlot()
        {
            _reservations.Reserve(new CarId(1), 0);
            Assert.IsTrue(_reservations.Release(new CarId(1)));
            Assert.IsFalse(_reservations.IsReserved(0));
        }

        [Test]
        public void Prune_DropsReservationsOfTicketsThatAreGoneOrParked()
        {
            var parked = _registry.Open(new CarId(1), new PatronRequest(30f, 20, false));
            var waiting = _registry.Open(new CarId(2), new PatronRequest(30f, 20, false));
            _reservations.Reserve(parked.CarId, 0);
            _reservations.Reserve(waiting.CarId, 1);
            _reservations.Reserve(new CarId(3), 2);
            _bus.Publish(new CarParked(parked.CarId, 1));
            _reservations.Prune(_registry);
            Assert.AreEqual(1, _reservations.Count);
            Assert.IsTrue(_reservations.IsReserved(1));
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
