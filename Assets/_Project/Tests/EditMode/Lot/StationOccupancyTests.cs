using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Lot;

namespace UncannyValet.Tests.Lot
{
    public class StationOccupancyTests
    {
        private readonly CarId _a = new CarId(1);
        private readonly CarId _b = new CarId(2);
        private StationOccupancy _station;

        [SetUp]
        public void SetUp() => _station = new StationOccupancy();

        [Test]
        public void EmptyStation_BlocksNobody()
        {
            Assert.IsFalse(_station.IsOccupied);
            Assert.IsFalse(_station.BlocksEntryOf(_a));
        }

        [Test]
        public void HeldStation_BlocksOtherCars_ButNotItsHolder()
        {
            _station.Admit(_a);
            Assert.IsTrue(_station.IsOccupied);
            Assert.IsTrue(_station.BlocksEntryOf(_b));
            Assert.IsFalse(_station.BlocksEntryOf(_a), "the car inside must be able to drive out");
        }

        [Test]
        public void ReleasedStation_AllowsEntryAgain()
        {
            _station.Admit(_a);
            Assert.IsTrue(_station.Release(_a));
            Assert.IsFalse(_station.BlocksEntryOf(_b));
        }

        [Test]
        public void AdmitAndReleaseAreIdempotent()
        {
            Assert.IsTrue(_station.Admit(_a));
            Assert.IsFalse(_station.Admit(_a));
            Assert.IsFalse(_station.Release(_b));
            Assert.IsTrue(_station.IsOccupied);
        }
    }
}
