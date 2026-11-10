using NUnit.Framework;
using UncannyValet.Traffic;

namespace UncannyValet.Tests.Traffic
{
    public class CarFollowingTests
    {
        private const float Dt = 0.02f;
        private static readonly FollowSettings Settings = new FollowSettings(5f, 0.6f, 0.8f, 3f, 6f);

        [Test]
        public void NoLeader_TargetsDesiredSpeed() =>
            Assert.AreEqual(5f, CarFollowing.TargetSpeed(float.PositiveInfinity, 0f, Settings), 0.001f);

        [Test]
        public void GapAtMinimum_TargetsZero() =>
            Assert.AreEqual(0f, CarFollowing.TargetSpeed(0.6f, 0f, Settings), 0.001f);

        [Test]
        public void BigGap_TargetsDesiredSpeed() =>
            Assert.AreEqual(5f, CarFollowing.TargetSpeed(50f, 5f, Settings), 0.001f);

        [Test]
        public void FreeRoad_AcceleratesAtTheAccelerationRate() =>
            Assert.AreEqual(1f + 3f * Dt, CarFollowing.NextSpeed(1f, float.PositiveInfinity, 0f, Settings, Dt), 0.001f);

        [Test]
        public void FollowerNeverReachesAStoppedLeader()
        {
            float position = 0f;
            float speed = 5f;
            const float leaderRear = 30f;
            for (int i = 0; i < 2000; i++)
            {
                float gap = leaderRear - position;
                speed = CarFollowing.NextSpeed(speed, gap, 0f, Settings, Dt);
                position += speed * Dt;
                Assert.Less(position, leaderRear, "car hit the leader");
            }
            Assert.AreEqual(0f, speed, 0.05f);
            Assert.GreaterOrEqual(leaderRear - position, Settings.MinGap - 0.05f);
        }

        [Test]
        public void Gap_ZeroOrNegative_StopsInstantly() =>
            Assert.AreEqual(0f, CarFollowing.NextSpeed(4f, -0.1f, 0f, Settings, Dt), 0.0001f);

        [Test]
        public void ApproachCap_FallsToEndSpeedAtThePoint()
        {
            Assert.AreEqual(2f, ApproachSpeed.Cap(0f, 2f, 6f), 0.001f);
            Assert.Greater(ApproachSpeed.Cap(10f, 2f, 6f), 2f);
        }
    }
}
