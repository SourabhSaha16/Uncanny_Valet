using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Tests.Pedestrians
{
    public class WanderPickerTests
    {
        private static readonly List<Vector2> Route = new List<Vector2>
        {
            new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(10f, 6f), new Vector2(4f, 6f)
        };

        [Test]
        public void TargetsNeverLandInsideAKeepOutRectangle()
        {
            var keepOut = new KeepOutAreas();
            keepOut.Add(new Rect(2f, -1f, 3f, 2f));
            keepOut.Add(new Rect(9f, 2f, 2f, 2f));
            var picker = new WanderPicker(0.05f, 8, keepOut, 7);
            var from = 0.5f;
            for (var i = 0; i < 300; i++)
            {
                if (!picker.TryPick(Route, from, out var to)) continue;
                Assert.IsFalse(keepOut.Contains(RoamPath.PointAt(Route, to)), "pick " + i + " fraction " + to);
                from = to;
            }
        }

        [Test]
        public void LegsNeverCrossAKeepOutRectangle()
        {
            var keepOut = new KeepOutAreas();
            keepOut.Add(new Rect(4f, -1f, 2f, 2f));
            var picker = new WanderPicker(0.05f, 8, keepOut, 3);
            var leg = new List<Vector2>();
            var from = 0.0f;
            for (var i = 0; i < 300; i++)
            {
                if (!picker.TryPick(Route, from, out var to)) { from = 0f; continue; }
                RoamPath.Slice(Route, from, to, leg);
                Assert.IsFalse(keepOut.Crosses(leg), "pick " + i);
                from = to;
            }
        }

        [Test]
        public void GivesUpWhenEveryRouteSpotIsBlocked()
        {
            var keepOut = new KeepOutAreas();
            keepOut.Add(new Rect(-100f, -100f, 200f, 200f));
            var picker = new WanderPicker(0.05f, 8, keepOut, 1);
            Assert.IsFalse(picker.TryPick(Route, 0.5f, out _));
        }

        [Test]
        public void LegsAreAtLeastTheMinimumShare()
        {
            var picker = new WanderPicker(0.3f, 16, new KeepOutAreas(), 11);
            var from = 0.4f;
            for (var i = 0; i < 200; i++)
            {
                Assert.IsTrue(picker.TryPick(Route, from, out var to));
                Assert.GreaterOrEqual(Mathf.Abs(to - from), 0.3f);
                from = to;
            }
        }

        [Test]
        public void SameSeedGivesTheSameSpots()
        {
            var a = new WanderPicker(0.1f, 8, new KeepOutAreas(), 5);
            var b = new WanderPicker(0.1f, 8, new KeepOutAreas(), 5);
            for (var i = 0; i < 20; i++)
            {
                a.TryPick(Route, 0.5f, out var x);
                b.TryPick(Route, 0.5f, out var y);
                Assert.AreEqual(x, y);
            }
        }

        [Test]
        public void SliceWalksEveryCornerBetweenInEitherDirection()
        {
            var leg = new List<Vector2>();
            RoamPath.Slice(Route, 0.1f, 0.95f, leg);
            Assert.AreEqual(4, leg.Count);
            Assert.AreEqual(new Vector2(10f, 0f), leg[1]);
            Assert.AreEqual(new Vector2(10f, 6f), leg[2]);
            RoamPath.Slice(Route, 0.95f, 0.1f, leg);
            Assert.AreEqual(new Vector2(10f, 6f), leg[1]);
            Assert.AreEqual(new Vector2(10f, 0f), leg[2]);
        }

        [Test]
        public void SegmentThroughARectangleIsCrossedEvenWithBothEndsOutside()
        {
            var keepOut = new KeepOutAreas();
            keepOut.Add(new Rect(1f, 1f, 2f, 2f));
            var through = new List<Vector2> { new Vector2(0f, 2f), new Vector2(4f, 2f) };
            var past = new List<Vector2> { new Vector2(0f, 4f), new Vector2(4f, 4f) };
            Assert.IsTrue(keepOut.Crosses(through));
            Assert.IsFalse(keepOut.Crosses(past));
        }
    }
}
