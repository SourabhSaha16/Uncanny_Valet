using NUnit.Framework;
using UncannyValet.Traffic;

namespace UncannyValet.Tests.Traffic
{
    public class SpawnGateTests
    {
        private const float CarLength = 2.2f;

        private readonly MergeGapRule _merge = new MergeGapRule(2.5f, 2.5f, 0.8f);
        private LaneOccupancy _occupancy;

        [SetUp]
        public void SetUp() => _occupancy = new LaneOccupancy(8);

        [Test]
        public void EmptyLane_IsOpen() =>
            Assert.IsTrue(SpawnGate.IsOpen(_occupancy, _merge, TrafficLane.EastBack, 0f, CarLength));

        [Test]
        public void CarOnTheSpot_ClosesTheLane()
        {
            _occupancy.Add(TrafficLane.EastBack, 0f, 5f, CarLength);
            Assert.IsFalse(SpawnGate.IsOpen(_occupancy, _merge, TrafficLane.EastBack, 0f, CarLength));
        }

        [Test]
        public void CarJustAhead_ClosesTheLane()
        {
            _occupancy.Add(TrafficLane.East, 4f, 5f, CarLength);
            Assert.IsFalse(SpawnGate.IsOpen(_occupancy, _merge, TrafficLane.East, 0f, CarLength));
        }

        [Test]
        public void FastCarJustBehind_ClosesTheLane()
        {
            _occupancy.Add(TrafficLane.East, -6f, 7f, CarLength);
            Assert.IsFalse(SpawnGate.IsOpen(_occupancy, _merge, TrafficLane.East, 0f, CarLength));
        }

        [Test]
        public void CarFarAway_LeavesTheLaneOpen()
        {
            _occupancy.Add(TrafficLane.East, 12f, 5f, CarLength);
            Assert.IsTrue(SpawnGate.IsOpen(_occupancy, _merge, TrafficLane.East, 0f, CarLength));
        }

        [Test]
        public void CarOnAnotherLane_IsIgnored()
        {
            _occupancy.Add(TrafficLane.East, 0f, 5f, CarLength);
            Assert.IsTrue(SpawnGate.IsOpen(_occupancy, _merge, TrafficLane.EastBack, 0f, CarLength));
        }
    }
}
