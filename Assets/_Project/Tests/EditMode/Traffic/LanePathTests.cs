using NUnit.Framework;
using UncannyValet.Traffic;
using UnityEngine;

namespace UncannyValet.Tests.Traffic
{
    public class LanePathTests
    {
        private static LanePath Make(params Vector2[] points)
        {
            var path = new LanePath();
            path.Rebuild(points);
            return path;
        }

        [Test]
        public void Length_SumsSegments() =>
            Assert.AreEqual(7f, Make(Vector2.zero, new Vector2(3f, 0f), new Vector2(3f, 4f)).Length, 0.001f);

        [Test]
        public void PositionAt_InterpolatesAcrossSegments()
        {
            LanePath path = Make(Vector2.zero, new Vector2(3f, 0f), new Vector2(3f, 4f));
            Assert.AreEqual(new Vector2(1f, 0f), path.PositionAt(1f));
            Assert.AreEqual(new Vector2(3f, 2f), path.PositionAt(5f));
        }

        [Test]
        public void PositionAt_ClampsToTheEnds()
        {
            LanePath path = Make(Vector2.zero, new Vector2(3f, 0f));
            Assert.AreEqual(Vector2.zero, path.PositionAt(-5f));
            Assert.AreEqual(new Vector2(3f, 0f), path.PositionAt(99f));
        }

        [Test]
        public void HeadingAt_UsesNoseUpConvention()
        {
            LanePath path = Make(Vector2.zero, new Vector2(3f, 0f), new Vector2(3f, 4f));
            Assert.AreEqual(-90f, path.HeadingAt(1f), 0.01f);
            Assert.AreEqual(0f, path.HeadingAt(5f), 0.01f);
        }

        [Test]
        public void Rebuild_ReplacesThePreviousPath()
        {
            LanePath path = Make(Vector2.zero, new Vector2(10f, 0f));
            path.Rebuild(new[] { Vector2.zero, new Vector2(0f, 2f) });
            Assert.AreEqual(2f, path.Length, 0.001f);
        }
    }
}
