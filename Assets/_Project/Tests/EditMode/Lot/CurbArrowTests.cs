using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
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

        private sealed class FakeAccept : IPickupAcceptance
        {
            public bool Yes;
            public bool WillAccept(CarId car) => Yes;
        }

        [Test]
        public void RightFacingButNotAcceptedIsIdle()
        {
            var t = new CurbArrowTracker();
            t.Set(new CarId(1), true);
            Assert.AreEqual(CurbArrowState.Idle, t.Evaluate(new FakeAccept()));
            Assert.AreEqual(CurbArrowState.Idle, t.Evaluate(null));
        }

        [Test]
        public void WrongFacingStaysWrongEvenIfAccepting()
        {
            var t = new CurbArrowTracker();
            t.Set(new CarId(1), false);
            Assert.AreEqual(CurbArrowState.Wrong, t.Evaluate(new FakeAccept { Yes = true }));
        }

        [Test]
        public void ArrowFlipsGreenWhenPatronAsksThenIdleWhenCarLeaves()
        {
            var t = new CurbArrowTracker();
            var accept = new FakeAccept();
            t.Set(new CarId(1), true);
            Assert.AreEqual(CurbArrowState.Idle, t.Evaluate(accept));
            accept.Yes = true;
            Assert.AreEqual(CurbArrowState.Right, t.Evaluate(accept));
            Assert.IsTrue(t.Remove(new CarId(1)));
            Assert.AreEqual(CurbArrowState.Idle, t.Evaluate(accept));
        }

        [Test]
        public void AcceptedAndWrongCarsTogetherShowRight()
        {
            var t = new CurbArrowTracker();
            t.Set(new CarId(1), true);
            t.Set(new CarId(2), false);
            Assert.AreEqual(CurbArrowState.Right, t.Evaluate(new FakeAccept { Yes = true }));
        }

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
