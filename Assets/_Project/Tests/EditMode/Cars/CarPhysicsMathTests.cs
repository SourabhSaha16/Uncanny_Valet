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
    }
}
