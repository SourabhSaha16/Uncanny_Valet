using NUnit.Framework;
using UncannyValet.Cars;

namespace UncannyValet.Tests.Cars
{
    public class CarAutoDriveMathTests
    {
        [Test]
        public void ArriveSpeedCruisesFarAwayAndEasesNearTheStop()
        {
            Assert.AreEqual(7f, CarAutoDriveMath.ArriveSpeed(14f, 7f, 6f), 1e-5f);
            Assert.Less(CarAutoDriveMath.ArriveSpeed(1f, 7f, 6f), 7f);
            Assert.AreEqual(0f, CarAutoDriveMath.ArriveSpeed(0f, 7f, 6f), 1e-6f);
        }

        [Test]
        public void ArriveStepNeverOvershoots()
        {
            Assert.LessOrEqual(CarAutoDriveMath.ArriveStep(0.01f, 7f, 6f, 1f), 0.01f);
            Assert.AreEqual(0f, CarAutoDriveMath.ArriveStep(-3f, 7f, 6f, 0.02f), 1e-6f);
        }

        [Test]
        public void SimulatedArrivalReachesTheStopPointInFiniteTime()
        {
            float remaining = 14f;
            int steps = 0;
            while (!CarAutoDriveMath.HasArrived(remaining) && steps < 5000)
            {
                remaining -= CarAutoDriveMath.ArriveStep(remaining, 7f, 6f, 0.02f);
                steps++;
            }
            Assert.Less(steps, 5000);
            Assert.GreaterOrEqual(remaining, 0f);
        }

        [Test]
        public void DepartSpeedAcceleratesAndCapsAtTop()
        {
            float speed = 0f;
            speed = CarAutoDriveMath.DepartSpeed(speed, 8f, 10f, 0.5f);
            Assert.AreEqual(4f, speed, 1e-5f);
            for (int i = 0; i < 20; i++) speed = CarAutoDriveMath.DepartSpeed(speed, 8f, 10f, 0.5f);
            Assert.AreEqual(10f, speed, 1e-5f);
        }

        [Test]
        public void HasLeftOnceTravelledEnough()
        {
            Assert.IsFalse(CarAutoDriveMath.HasLeft(15.9f, 16f));
            Assert.IsTrue(CarAutoDriveMath.HasLeft(16f, 16f));
        }
    }
}
