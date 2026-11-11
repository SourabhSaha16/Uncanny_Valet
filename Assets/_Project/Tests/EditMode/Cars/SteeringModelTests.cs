using NUnit.Framework;
using UncannyValet.Cars;

namespace UncannyValet.Tests.Cars
{
    public class SteeringModelTests
    {
        private const float RMin = 3.3f;
        private const float FullSpeed = 4f;
        private static readonly float MaxRate = TurningRadius.MaxTurnRate(FullSpeed, RMin);

        [Test]
        public void NoRotationAtZeroSpeed() => Assert.AreEqual(0f, SteeringModel.TurnRate(1f, 0f, MaxRate, FullSpeed), 1e-5f);

        [Test]
        public void RightSteerTurnsClockwiseAndLeftCounterClockwise()
        {
            Assert.Less(SteeringModel.TurnRate(1f, 3f, MaxRate, FullSpeed), 0f);
            Assert.Greater(SteeringModel.TurnRate(-1f, 3f, MaxRate, FullSpeed), 0f);
        }

        [Test]
        public void FullSpeedAndAboveGiveMaxTurnRate()
        {
            Assert.AreEqual(MaxRate, SteeringModel.TurnRate(-1f, FullSpeed, MaxRate, FullSpeed), 1e-4f);
            Assert.AreEqual(MaxRate, SteeringModel.TurnRate(-1f, 8f, MaxRate, FullSpeed), 1e-4f);
        }

        [Test]
        public void RadiusEqualsRMinBelowFullSpeed()
        {
            foreach (var speed in new[] { 0.5f, 1f, 2f, 4f })
            {
                var radius = speed / SteeringModel.TurnRate(-1f, speed, MaxRate, FullSpeed);
                Assert.AreEqual(RMin, radius, 1e-3f, "speed " + speed);
                Assert.AreEqual(RMin, SteeringModel.Radius(speed, RMin, MaxRate), 1e-3f);
            }
        }

        [Test]
        public void RadiusGrowsAboveFullSpeed()
        {
            var radius = 6f / SteeringModel.TurnRate(-1f, 6f, MaxRate, FullSpeed);
            Assert.AreEqual(6f / MaxRate, radius, 1e-3f);
            Assert.AreEqual(radius, SteeringModel.Radius(6f, RMin, MaxRate), 1e-3f);
            Assert.Greater(radius, RMin);
        }

        [Test]
        public void SteeringFlipsInReverse()
        {
            Assert.AreEqual(-SteeringModel.TurnRate(1f, 3f, MaxRate, FullSpeed), SteeringModel.TurnRate(1f, -3f, MaxRate, FullSpeed), 1e-5f);
        }

        [Test]
        public void SteerInputIsClamped() => Assert.AreEqual(-MaxRate, SteeringModel.TurnRate(5f, 8f, MaxRate, FullSpeed), 1e-4f);
    }
}
