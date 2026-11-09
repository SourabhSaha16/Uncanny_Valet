using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Tests.Pedestrians
{
    public class PathFollowerTests
    {
        private static List<Vector2> L() => new List<Vector2> { Vector2.zero, new Vector2(4f, 0f), new Vector2(4f, 3f) };

        [Test]
        public void WalksAroundCornersAndFinishesOnce()
        {
            var walker = new PathFollower();
            walker.Begin(L());
            Assert.IsTrue(walker.Walking);
            Assert.AreEqual(7f, walker.Length, 1e-5f);
            Assert.IsFalse(walker.Advance(5f));
            Assert.AreEqual(new Vector2(4f, 1f).x, walker.Position.x, 1e-4f);
            Assert.AreEqual(1f, walker.Position.y, 1e-4f);
            Assert.AreEqual(Vector2.up, walker.Heading);
            Assert.IsTrue(walker.Advance(10f));
            Assert.IsFalse(walker.Walking);
            Assert.AreEqual(new Vector2(4f, 3f), walker.Position);
            Assert.IsFalse(walker.Advance(1f));
        }

        [Test]
        public void StartFractionSkipsAlongTheRoute()
        {
            var walker = new PathFollower();
            walker.Begin(L(), 0.5f);
            Assert.AreEqual(3.5f, walker.Position.x, 0.2f);
            Assert.IsTrue(walker.Walking);
        }

        [Test]
        public void SinglePointOnlyPlacesTheWalker()
        {
            var walker = new PathFollower();
            walker.Begin(new List<Vector2> { new Vector2(2f, 2f) });
            Assert.IsFalse(walker.Walking);
            Assert.AreEqual(new Vector2(2f, 2f), walker.Position);
        }

        [Test]
        public void RouteIsCopiedSoTheCallerCanReuseItsList()
        {
            var route = L();
            var walker = new PathFollower();
            walker.Begin(route);
            route.Clear();
            Assert.IsFalse(walker.Advance(1f));
            Assert.AreEqual(1f, walker.Position.x, 1e-4f);
        }

        [Test]
        public void ZeroLengthSegmentsDoNotStall()
        {
            var walker = new PathFollower();
            walker.Begin(new List<Vector2> { Vector2.zero, Vector2.zero, new Vector2(1f, 0f) });
            Assert.IsTrue(walker.Advance(5f));
            Assert.AreEqual(1f, walker.Position.x, 1e-4f);
        }
    }
}
