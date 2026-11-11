using NUnit.Framework;
using UncannyValet.Traffic;

namespace UncannyValet.Tests.Traffic
{
    public class LaneOccupancyTests
    {
        private const float Length = 2f;

        [Test]
        public void EmptyLane_GapsAreInfinite()
        {
            var occupancy = new LaneOccupancy(4);
            Assert.IsTrue(float.IsPositiveInfinity(occupancy.GapAhead(TrafficLane.East, 5f, Length, 0, out _)));
            Assert.IsTrue(float.IsPositiveInfinity(occupancy.GapBehind(TrafficLane.East, 5f, Length, 0, out _)));
        }

        [Test]
        public void GapAhead_IsBumperToBumper_AndReportsLeaderSpeed()
        {
            var occupancy = new LaneOccupancy(4);
            occupancy.Add(TrafficLane.East, 10f, 3f, Length);
            float gap = occupancy.GapAhead(TrafficLane.East, 5f, Length, 0, out float speed);
            Assert.AreEqual(3f, gap, 0.001f);
            Assert.AreEqual(3f, speed, 0.001f);
        }

        [Test]
        public void GapBehind_FindsTheNearestFollower()
        {
            var occupancy = new LaneOccupancy(4);
            occupancy.Add(TrafficLane.East, 0f, 1f, Length);
            occupancy.Add(TrafficLane.East, 3f, 4f, Length);
            float gap = occupancy.GapBehind(TrafficLane.East, 8f, Length, 0, out float speed);
            Assert.AreEqual(3f, gap, 0.001f);
            Assert.AreEqual(4f, speed, 0.001f);
        }

        [Test]
        public void OtherLane_IsIgnored()
        {
            var occupancy = new LaneOccupancy(4);
            occupancy.Add(TrafficLane.Top, 10f, 3f, Length);
            Assert.IsTrue(float.IsPositiveInfinity(occupancy.GapAhead(TrafficLane.East, 5f, Length, 0, out _)));
        }

        [Test]
        public void ExcludedId_IsIgnored()
        {
            var occupancy = new LaneOccupancy(4);
            int id = occupancy.Add(TrafficLane.East, 10f, 3f, Length);
            Assert.IsTrue(float.IsPositiveInfinity(occupancy.GapAhead(TrafficLane.East, 5f, Length, id, out _)));
        }

        [Test]
        public void UpdateAndRemove_ChangeWhatIsSeen()
        {
            var occupancy = new LaneOccupancy(4);
            int id = occupancy.Add(TrafficLane.East, 10f, 3f, Length);
            occupancy.Update(id, 20f, 1f);
            Assert.AreEqual(13f, occupancy.GapAhead(TrafficLane.East, 5f, Length, 0, out _), 0.001f);
            occupancy.Remove(id);
            Assert.AreEqual(0, occupancy.Count);
        }
    }
}
