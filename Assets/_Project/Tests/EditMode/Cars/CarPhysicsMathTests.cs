using NUnit.Framework;
using UncannyValet.Cars;

namespace UncannyValet.Tests.Cars
{
    public class CarPhysicsMathTests
    {
        private static float Step(float forward, float throttle)
            => CarPhysicsMath.NextForwardSpeed(forward, throttle, 6f, 12f, 3f, 8f, 0.5f, 0.1f);

        [Test]
        public void ThrottleAcceleratesAndClampsAtTopSpeed()
        {
            Assert.AreEqual(0.6f, Step(0f, 1f), 1e-4f);
            Assert.AreEqual(8f, Step(7.9f, 1f), 1e-4f);
        }

        [Test]
        public void ReverseIsLimitedByReverseFactor()
        {
            Assert.AreEqual(-4f, Step(-3.9f, -1f), 1e-4f);
        }

        [Test]
        public void OpposingThrottleBrakesHarderThanCoasting()
        {
            Assert.AreEqual(3.8f, Step(5f, -1f), 1e-4f);
            Assert.AreEqual(4.7f, Step(5f, 0f), 1e-4f);
        }

        [Test]
        public void CoastingStopsAtZeroWithoutOvershoot()
        {
            Assert.AreEqual(0f, Step(0.1f, 0f), 1e-4f);
            Assert.AreEqual(0f, Step(-0.1f, 0f), 1e-4f);
        }

        [Test]
        public void NoTurnAtRestAndSignFollowsSteer()
        {
            Assert.AreEqual(0f, CarPhysicsMath.TurnDegreesPerSecond(1f, 0f, 120f, 8f), 1e-4f);
            Assert.Less(CarPhysicsMath.TurnDegreesPerSecond(1f, 8f, 120f, 8f), 0f);
            Assert.Greater(CarPhysicsMath.TurnDegreesPerSecond(-1f, 8f, 120f, 8f), 0f);
        }

        [Test]
        public void TurnScalesWithSpeedAndSaturates()
        {
            Assert.AreEqual(60f, CarPhysicsMath.TurnDegreesPerSecond(-1f, 1.2f, 120f, 8f), 1e-3f);
            Assert.AreEqual(120f, CarPhysicsMath.TurnDegreesPerSecond(-1f, 8f, 120f, 8f), 1e-3f);
        }

        [Test]
        public void ReversingInvertsTurnDirection()
        {
            Assert.AreEqual(-CarPhysicsMath.TurnDegreesPerSecond(1f, 4f, 120f, 8f),
                CarPhysicsMath.TurnDegreesPerSecond(1f, -4f, 120f, 8f), 1e-4f);
        }

        [Test]
        public void GripDampsSidewaysSpeedAndZeroGripKeepsIt()
        {
            Assert.AreEqual(0f, CarPhysicsMath.DampSideways(5f, 100f, 1f), 1e-3f);
            Assert.AreEqual(5f, CarPhysicsMath.DampSideways(5f, 0f, 1f), 1e-4f);
            Assert.Less(CarPhysicsMath.DampSideways(5f, 6f, 0.02f), 5f);
        }
    }
}
