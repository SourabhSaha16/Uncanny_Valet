using NUnit.Framework;
using UncannyValet.Cars;

namespace UncannyValet.Tests.Cars
{
    public class CarTimerFillMathTests
    {
        [Test]
        public void SmoothMovesTowardTargetWithoutOvershoot()
        {
            float next = CarTimerFillMath.Smooth(0f, 0.5f, 4f, 0.1f);
            Assert.Greater(next, 0f);
            Assert.Less(next, 0.5f);
        }

        [Test]
        public void SmoothSettlesExactlyOnTarget()
        {
            float shown = 0f;
            for (int i = 0; i < 600; i++) shown = CarTimerFillMath.Smooth(shown, 0.4f, 4f, 0.016f);
            Assert.AreEqual(0.4f, shown);
        }

        [Test]
        public void SmoothIsFrameRateIndependent()
        {
            float coarse = CarTimerFillMath.Smooth(0f, 1f, 3f, 0.2f);
            float fine = 0f;
            for (int i = 0; i < 4; i++) fine = CarTimerFillMath.Smooth(fine, 1f, 3f, 0.05f);
            Assert.AreEqual(coarse, fine, 1e-4f);
        }

        [Test]
        public void SmoothClampsTargetAndCurrent()
        {
            Assert.AreEqual(1f, CarTimerFillMath.Smooth(0.9f, 5f, 1000f, 1f));
            Assert.AreEqual(0f, CarTimerFillMath.Smooth(-3f, -2f, 4f, 0.1f));
        }

        [Test]
        public void ZeroRateOrNegativeDtDoesNotMove()
        {
            Assert.AreEqual(0.2f, CarTimerFillMath.Smooth(0.2f, 0.8f, 0f, 0.1f), 1e-6f);
            Assert.AreEqual(0.2f, CarTimerFillMath.Smooth(0.2f, 0.8f, 4f, -1f), 1e-6f);
        }
    }
}
