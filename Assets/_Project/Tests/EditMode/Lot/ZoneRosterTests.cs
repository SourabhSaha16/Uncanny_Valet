using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Lot;

namespace UncannyValet.Tests.Lot
{
    public class ZoneRosterTests
    {
        private readonly List<CarId> _settled = new List<CarId>();
        private readonly List<(CarId id, bool wasSettled)> _gone = new List<(CarId id, bool wasSettled)>();
        private static readonly CarId Car = new CarId(7);

        private void Tick(ZoneRoster roster, float dt, CarReading reading)
        {
            _settled.Clear();
            _gone.Clear();
            roster.Tick(dt, _ => reading, _settled, _gone);
        }

        [Test]
        public void StoppedCar_Settles_AfterTheRequiredTime()
        {
            var roster = new ZoneRoster(0.5f);
            roster.Enter(Car);
            Tick(roster, 0.3f, CarReading.Stopped);
            Assert.IsEmpty(_settled);
            Tick(roster, 0.3f, CarReading.Stopped);
            Assert.AreEqual(new[] { Car }, _settled);
        }

        [Test]
        public void DestroyedAfterSettling_IsReleasedAsSettled()
        {
            var roster = new ZoneRoster(0.5f);
            roster.Enter(Car);
            Tick(roster, 0.6f, CarReading.Stopped);
            Tick(roster, 0.1f, CarReading.Gone);
            Assert.AreEqual(1, _gone.Count);
            Assert.AreEqual(Car, _gone[0].id);
            Assert.IsTrue(_gone[0].wasSettled);
            Assert.AreEqual(0, roster.Count);
        }

        [Test]
        public void DestroyedBeforeSettling_IsDroppedWithoutLeaveEvent()
        {
            var roster = new ZoneRoster(0.5f);
            roster.Enter(Car);
            Tick(roster, 0.1f, CarReading.Gone);
            Assert.IsFalse(_gone[0].wasSettled);
            Assert.AreEqual(0, roster.Count);
        }

        [Test]
        public void Exit_ReportsWhetherCarHadSettled()
        {
            var roster = new ZoneRoster(0.5f);
            roster.Enter(Car);
            Assert.IsFalse(roster.Exit(Car));
            roster.Enter(Car);
            Tick(roster, 0.6f, CarReading.Stopped);
            Assert.IsTrue(roster.Exit(Car));
            Assert.IsFalse(roster.Exit(Car));
        }

        [Test]
        public void Drain_ReturnsOnlySettledCars_AndEmptiesTheRoster()
        {
            var roster = new ZoneRoster(0.5f);
            var other = new CarId(8);
            roster.Enter(Car);
            roster.Enter(other);
            roster.Tick(0.6f, id => id.Equals(Car) ? CarReading.Stopped : CarReading.Active, _settled, _gone);
            var released = new List<CarId>();
            roster.Drain(released);
            Assert.AreEqual(new[] { Car }, released);
            Assert.AreEqual(0, roster.Count);
        }

        [Test]
        public void Drain_ThenExit_GivesNoSecondLeave()
        {
            var roster = new ZoneRoster(0.5f);
            roster.Enter(Car);
            Tick(roster, 0.6f, CarReading.Stopped);
            roster.Drain(new List<CarId>());
            Assert.IsFalse(roster.Exit(Car));
        }

        [Test]
        public void StationCar_SettlesOnce_ThenDestroyedInside_LeavesExactlyOnce()
        {
            var roster = new ZoneRoster(0.5f);
            roster.Enter(Car);
            Tick(roster, 0.6f, CarReading.Stopped);
            Assert.AreEqual(1, _settled.Count);
            Tick(roster, 0.1f, CarReading.Stopped);
            Assert.IsEmpty(_settled);
            Tick(roster, 0.1f, CarReading.Gone);
            Assert.AreEqual(1, _gone.Count);
            Assert.IsTrue(_gone[0].wasSettled);
            Tick(roster, 0.1f, CarReading.Gone);
            Assert.IsEmpty(_gone);
        }

        [Test]
        public void ReleasedSpot_IsFreeAgainThroughOccupancy()
        {
            var occupancy = new SpotOccupancy(new[] { 1 });
            var roster = new ZoneRoster(0.5f);
            roster.Enter(Car);
            Tick(roster, 0.6f, CarReading.Stopped);
            occupancy.Park(1, Car);
            Tick(roster, 0.1f, CarReading.Gone);
            foreach (var g in _gone) if (g.wasSettled) occupancy.Leave(1, g.id);
            Assert.IsTrue(occupancy.IsFree(1));
        }
    }
}
