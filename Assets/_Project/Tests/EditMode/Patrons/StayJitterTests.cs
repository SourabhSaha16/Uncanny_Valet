using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    /// <summary>Issue 144: returns were stacked; each stay now moves by a seeded random fraction.</summary>
    public class StayJitterTests
    {
        private const float Fraction = 0.3f;
        private const float Floor = 8f;
        private const float Stay = 30f;
        private const int Cars = 500;

        [Test]
        public void ManyCars_GetVariedStays()
        {
            var distinct = new HashSet<float>();
            var min = float.MaxValue;
            var max = float.MinValue;
            for (var id = 1; id <= Cars; id++)
            {
                var s = StayJitter.Apply(Stay, Fraction, Floor, id);
                distinct.Add(s);
                if (s < min) min = s;
                if (s > max) max = s;
            }
            Assert.Greater(distinct.Count, Cars / 2);
            Assert.Less(min, Stay * 0.8f);
            Assert.Greater(max, Stay * 1.2f);
        }

        [Test]
        public void EveryStay_StaysWithinTheFraction()
        {
            for (var id = 1; id <= Cars; id++)
            {
                var s = StayJitter.Apply(Stay, Fraction, Floor, id);
                Assert.GreaterOrEqual(s, Stay * (1f - Fraction) - 0.001f);
                Assert.LessOrEqual(s, Stay * (1f + Fraction) + 0.001f);
            }
        }

        [Test]
        public void SmallStay_NeverGoesBelowTheFloor_OrNegative()
        {
            for (var id = 1; id <= Cars; id++)
            {
                Assert.GreaterOrEqual(StayJitter.Apply(10f, 0.9f, Floor, id), Floor);
                Assert.GreaterOrEqual(StayJitter.Apply(3f, 0.9f, Floor, id), 0f);
            }
        }

        [Test]
        public void StayShorterThanFloor_IsNeverLengthened()
        {
            Assert.AreEqual(3f, StayJitter.Apply(3f, 0.9f, Floor, 7), 0.0001f);
        }

        [Test]
        public void SameId_GivesTheSameStay()
        {
            Assert.AreEqual(StayJitter.Apply(Stay, Fraction, Floor, 42), StayJitter.Apply(Stay, Fraction, Floor, 42));
        }

        [Test]
        public void ZeroFraction_ChangesNothing()
        {
            Assert.AreEqual(Stay, StayJitter.Apply(Stay, 0f, Floor, 5));
        }

        [Test]
        public void CarsParkedTogether_AskForPickupSpreadOut()
        {
            // Six cars parked at the same minute with the same rolled stay, then fitted to the shift as the spawner does.
            const float remaining = 200f;
            const float lead = 25f;
            var returns = new List<float>();
            for (var id = 1; id <= 6; id++)
            {
                var spread = StayJitter.Apply(Stay, Fraction, Floor, id);
                var fitted = StayFit.Clamp(spread, remaining, lead, Floor);
                Assert.LessOrEqual(fitted, remaining - lead);
                returns.Add(fitted);
            }
            returns.Sort();
            Assert.Greater(returns[returns.Count - 1] - returns[0], Stay * 0.2f);
        }

        [Test]
        public void LateArrival_StillFitsTheShift()
        {
            for (var id = 1; id <= Cars; id++)
            {
                var fitted = StayFit.Clamp(StayJitter.Apply(45f, Fraction, Floor, id), 60f, 25f, Floor);
                Assert.LessOrEqual(fitted, 35f);
            }
        }
    }
}
