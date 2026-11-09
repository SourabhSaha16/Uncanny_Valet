using NUnit.Framework;
using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Tests.Pedestrians
{
    public class RouteWalkerTests
    {
        private static readonly Vector2[] Corner =
        {
            new Vector2(0f, 0f), new Vector2(4f, 0f), new Vector2(4f, 3f)
        };

        [Test]
        public void StartsAtTheFirstPointHeadingToTheSecond()
        {
            var walker = new RouteWalker(Corner);
            Assert.AreEqual(Vector2.zero, walker.Position);
            Assert.AreEqual(Vector2.right, walker.Heading);
            Assert.AreEqual(7f, walker.Length, 1e-4f);
            Assert.AreEqual(14f, walker.CycleLength, 1e-4f);
        }

        [Test]
        public void AdvanceWalksAlongTheSegment()
        {
            var walker = new RouteWalker(Corner);
            walker.Advance(1.5f);
            Assert.AreEqual(1.5f, walker.Position.x, 1e-4f);
            Assert.AreEqual(0f, walker.Position.y, 1e-4f);
        }

        [Test]
        public void AdvanceTurnsTheCornerWithoutLosingDistance()
        {
            var walker = new RouteWalker(Corner);
            walker.Advance(5f); // 4 along the first leg, 1 up the second
            Assert.AreEqual(4f, walker.Position.x, 1e-4f);
            Assert.AreEqual(1f, walker.Position.y, 1e-4f);
            Assert.AreEqual(Vector2.up, walker.Heading);
        }

        [Test]
        public void AtTheEndTheWalkerTurnsRoundAndComesBack()
        {
            var walker = new RouteWalker(Corner);
            walker.Advance(8f); // 7 to the end, 1 back
            Assert.AreEqual(4f, walker.Position.x, 1e-4f);
            Assert.AreEqual(2f, walker.Position.y, 1e-4f);
            Assert.AreEqual(Vector2.down, walker.Heading);
        }

        [Test]
        public void AFullCycleReturnsToTheStart()
        {
            var walker = new RouteWalker(Corner);
            walker.Advance(walker.CycleLength);
            Assert.AreEqual(0f, walker.Position.x, 1e-3f);
            Assert.AreEqual(0f, walker.Position.y, 1e-3f);
            Assert.AreEqual(Vector2.right, walker.Heading);
        }

        [Test]
        public void ManySmallStepsMatchOneBigStep()
        {
            var steps = new RouteWalker(Corner);
            for (var i = 0; i < 100; i++) steps.Advance(0.1f);
            var jump = new RouteWalker(Corner);
            jump.Advance(10f);
            Assert.AreEqual(jump.Position.x, steps.Position.x, 1e-2f);
            Assert.AreEqual(jump.Position.y, steps.Position.y, 1e-2f);
        }

        [Test]
        public void StartAtWrapsWithinOneCycle()
        {
            var a = new RouteWalker(Corner);
            a.StartAt(3f);
            var b = new RouteWalker(Corner);
            b.StartAt(3f + b.CycleLength * 2f);
            Assert.AreEqual(a.Position.x, b.Position.x, 1e-3f);
            Assert.AreEqual(a.Position.y, b.Position.y, 1e-3f);
        }

        [Test]
        public void NegativeDistanceDoesNothing()
        {
            var walker = new RouteWalker(Corner);
            walker.Advance(-3f);
            Assert.AreEqual(Vector2.zero, walker.Position);
        }

        [Test]
        public void ARouteWithoutTwoPointsStandsStill()
        {
            var single = new RouteWalker(new[] { new Vector2(2f, 2f) });
            single.Advance(5f);
            Assert.IsFalse(single.CanWalk);
            Assert.AreEqual(new Vector2(2f, 2f), single.Position);
            Assert.AreEqual(Vector2.zero, single.Heading);
            Assert.DoesNotThrow(() => new RouteWalker(null).Advance(1f));
        }

        [Test]
        public void RepeatedPointsDoNotHangTheWalker()
        {
            var walker = new RouteWalker(new[] { Vector2.zero, Vector2.zero, Vector2.zero });
            Assert.DoesNotThrow(() => walker.Advance(10f));
        }
    }
}
