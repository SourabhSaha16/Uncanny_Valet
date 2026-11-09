using NUnit.Framework;
using UncannyValet.Cars;

namespace UncannyValet.Tests.Cars
{
    public class PickupFlashMathTests
    {
        [Test]
        public void FrequencyRisesFromMinToMax()
        {
            Assert.AreEqual(1f, PickupFlashMath.Frequency(0f, 1f, 6f), 1e-4f);
            Assert.AreEqual(3.5f, PickupFlashMath.Frequency(0.5f, 1f, 6f), 1e-4f);
            Assert.AreEqual(6f, PickupFlashMath.Frequency(1f, 1f, 6f), 1e-4f);
        }

        [Test]
        public void FrequencyIsClampedOutsideZeroToOne()
        {
            Assert.AreEqual(1f, PickupFlashMath.Frequency(-3f, 1f, 6f), 1e-4f);
            Assert.AreEqual(6f, PickupFlashMath.Frequency(9f, 1f, 6f), 1e-4f);
        }

        [Test]
        public void PhaseWrapsAndNeverGoesBackwards()
        {
            Assert.AreEqual(0.25f, PickupFlashMath.Advance(0.75f, 1f, 0.5f), 1e-4f);
            Assert.AreEqual(0.4f, PickupFlashMath.Advance(0.4f, -2f, 1f), 1e-4f);
            Assert.AreEqual(0.4f, PickupFlashMath.Advance(0.4f, 2f, -1f), 1e-4f);
        }

        [Test]
        public void FasterFlashSwitchesMoreOftenInOneSecond()
        {
            Assert.Greater(Switches(6f), Switches(1f));
        }

        [Test]
        public void LevelsFollowThePatience()
        {
            Assert.AreEqual(FlashLevel.Calm, PickupFlashMath.Level(0.2f, false, 0.6f, 0.85f));
            Assert.AreEqual(FlashLevel.Amber, PickupFlashMath.Level(0.7f, false, 0.6f, 0.85f));
            Assert.AreEqual(FlashLevel.Danger, PickupFlashMath.Level(0.9f, false, 0.6f, 0.85f));
            Assert.AreEqual(FlashLevel.Danger, PickupFlashMath.Level(0.1f, true, 0.6f, 0.85f));
        }

        [Test]
        public void IsOnForTheFirstHalfOnly()
        {
            Assert.IsTrue(PickupFlashMath.IsOn(0f));
            Assert.IsTrue(PickupFlashMath.IsOn(0.49f));
            Assert.IsFalse(PickupFlashMath.IsOn(0.5f));
        }

        private static int Switches(float hz)
        {
            var phase = 0f;
            var on = PickupFlashMath.IsOn(phase);
            var count = 0;
            for (var i = 0; i < 100; i++)
            {
                phase = PickupFlashMath.Advance(phase, hz, 0.01f);
                var now = PickupFlashMath.IsOn(phase);
                if (now != on) count++;
                on = now;
            }
            return count;
        }
    }
}
