using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class CurbArrowTests
    {
        [Test]
        public void NoCarsIsIdle() => Assert.AreEqual(CurbArrowState.Idle, CurbArrowRule.Evaluate(0, 0));

        [Test]
        public void OnlyWrongCarsIsWrong() => Assert.AreEqual(CurbArrowState.Wrong, CurbArrowRule.Evaluate(0, 2));

        [Test]
        public void AnyRightCarIsRight() => Assert.AreEqual(CurbArrowState.Right, CurbArrowRule.Evaluate(1, 1));

        [Test]
        public void RequiredDirectionPointsUpTheScreen()
        {
            var d = CurbFacingRule.RequiredDirection;
            Assert.AreEqual(0f, d.x, 1e-4f);
            Assert.AreEqual(1f, d.y, 1e-4f);
        }

        [Test]
        public void RequiredDirectionIsWhatTheRuleAccepts()
        {
            var d = CurbFacingRule.RequiredDirection;
            var degrees = Mathf.Atan2(-d.x, d.y) * Mathf.Rad2Deg;
            Assert.IsTrue(new CurbFacingRule(1f).IsAcceptable(degrees));
        }

        [Test]
        public void TextHalfExtentCoversOuterColumnAndHalfALetter()
            => Assert.AreEqual(0.8f, CurbArrowLayout.TextHalfExtent(2, 0.95f, 0.65f), 1e-4f);

        [Test]
        public void SideXSitsBetweenTextAndEdge()
        {
            var x = CurbArrowLayout.SideX(3f, 0.8f, 0.2f);
            Assert.AreEqual(1.05f, x, 1e-4f);
        }
    }
}
