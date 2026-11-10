using NUnit.Framework;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    /// <summary>Regression: a car sent late in the shift used to get a stay that ran past the end, so its patron never came back.</summary>
    public class StayFitTests
    {
        private const float Lead = 25f;
        private const float Floor = 8f;

        [Test]
        public void EarlyArrival_KeepsItsStay()
        {
            Assert.AreEqual(40f, StayFit.Clamp(40f, 200f, Lead, Floor));
        }

        [Test]
        public void LateArrival_IsCutToWhatFitsBeforeTheLead()
        {
            Assert.AreEqual(35f, StayFit.Clamp(45f, 60f, Lead, Floor));
        }

        [Test]
        public void VeryLateArrival_NeverGoesBelowTheFloor()
        {
            Assert.AreEqual(Floor, StayFit.Clamp(45f, 10f, Lead, Floor));
        }

        [Test]
        public void AShortRolledStay_IsNeverRaised()
        {
            Assert.AreEqual(5f, StayFit.Clamp(5f, 10f, Lead, Floor));
        }
    }
}
