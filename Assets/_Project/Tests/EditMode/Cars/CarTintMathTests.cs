using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core.Config;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class CarTintMathTests
    {
        private static readonly Color Red = new Color(0.9f, 0.1f, 0.1f, 1f);

        private static void AssertColor(Color expected, Color actual)
        {
            Assert.AreEqual(expected.r, actual.r, 1e-5f);
            Assert.AreEqual(expected.g, actual.g, 1e-5f);
            Assert.AreEqual(expected.b, actual.b, 1e-5f);
            Assert.AreEqual(expected.a, actual.a, 1e-5f);
        }

        [Test]
        public void ZeroSoftnessKeepsTheTagColourAndFullSoftnessIsWhite()
        {
            AssertColor(Red, CarTintMath.Soften(Red, 0f));
            AssertColor(Color.white, CarTintMath.Soften(Red, 1f));
        }

        [Test]
        public void SoftnessMovesTowardWhiteAndIsClamped()
        {
            AssertColor(new Color(0.95f, 0.55f, 0.55f, 1f), CarTintMath.Soften(Red, 0.5f));
            AssertColor(Color.white, CarTintMath.Soften(Red, 7f));
            AssertColor(Red, CarTintMath.Soften(Red, -3f));
        }

        [Test]
        public void SoftenedTagsStayBrighterThanTheRawColour()
        {
            for (var tag = 0; tag < TagPalette.Count; tag++)
            {
                var raw = TagPalette.Get(tag);
                var soft = CarTintMath.Soften(raw, 0.45f);
                Assert.GreaterOrEqual(soft.r, raw.r);
                Assert.GreaterOrEqual(soft.g, raw.g);
                Assert.GreaterOrEqual(soft.b, raw.b);
            }
        }

        [Test]
        public void RegularCarGetsTheSoftenedTint()
        {
            AssertColor(CarTintMath.Soften(Red, 0.4f), CarTintMath.BodyTint(Red, 0.4f, false, 0.25f));
        }

        [Test]
        public void EvGetsOnlyAShareOfTheTint()
        {
            var full = CarTintMath.Soften(Red, 0.4f);
            AssertColor(Color.white, CarTintMath.BodyTint(Red, 0.4f, true, 0f));
            AssertColor(full, CarTintMath.BodyTint(Red, 0.4f, true, 1f));
            AssertColor(Color.Lerp(Color.white, full, 0.25f), CarTintMath.BodyTint(Red, 0.4f, true, 0.25f));
        }

        [Test]
        public void UnknownTagTintsAlmostNothingWhenWhite()
        {
            AssertColor(Color.white, CarTintMath.BodyTint(Color.white, 0.45f, false, 0.25f));
        }

        [Test]
        public void PulseOffLeavesTheTintAndOnMovesTowardTheFlash()
        {
            var flash = new Color(0.2f, 0.8f, 0.9f, 1f);
            AssertColor(Red, CarTintMath.Pulse(Red, flash, 0.6f, false));
            AssertColor(Color.Lerp(Red, flash, 0.6f), CarTintMath.Pulse(Red, flash, 0.6f, true));
            AssertColor(flash, CarTintMath.Pulse(Red, flash, 5f, true));
        }
    }
}
