using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Cars.Fx;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class TyreBendTests
    {
        private const float Spacing = 1f;
        private const float MaxBend = 10f;
        private const float MinLength = 0.2f;

        private static List<MarkSegment> Run(TyreTrail trail, int steps, System.Func<int, Vector2> path)
        {
            var strokes = new List<MarkSegment>();
            for (var i = 0; i < steps; i++)
                if (trail.TryAdvance(path(i), Spacing, 0f, MaxBend, MinLength, out var s)) strokes.Add(s);
            return strokes;
        }

        private static Vector2 Circle(int i, float radius)
        {
            var a = i * 0.05f;
            return new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        }

        private static Vector2 Dir(MarkSegment s)
        {
            var rad = (s.AngleDegrees + 90f) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        [Test]
        public void Straight_StillCutsAtOneUnit()
        {
            var strokes = Run(new TyreTrail(), 101, i => new Vector2(i * 0.1f, 0f));
            Assert.AreEqual(10, strokes.Count);
            foreach (var s in strokes) Assert.AreEqual(1f, s.Length, 0.11f);
        }

        [Test]
        public void Circle_ChordsTurnNoMoreThanTheMaxAngle()
        {
            var strokes = Run(new TyreTrail(), 200, i => Circle(i, 2f));
            Assert.Greater(strokes.Count, 10);
            for (var i = 1; i < strokes.Count; i++)
                Assert.LessOrEqual(Vector2.Angle(Dir(strokes[i - 1]), Dir(strokes[i])), MaxBend + 4f);
        }

        [Test]
        public void NoStrokeIsShorterThanTheMinimumDistance()
        {
            var strokes = Run(new TyreTrail(), 400, i => Circle(i, 1f));
            Assert.Greater(strokes.Count, 10);
            foreach (var s in strokes) Assert.GreaterOrEqual(s.Length, MinLength - 1e-4f);
        }

        [Test]
        public void Break_ResetsTheBendReference()
        {
            var trail = new TyreTrail();
            Run(trail, 60, i => Circle(i, 2f));
            trail.Break();
            Assert.IsFalse(trail.TryAdvance(new Vector2(50f, 50f), Spacing, 0f, MaxBend, MinLength, out _));
            Assert.IsFalse(trail.TryAdvance(new Vector2(50.3f, 50f), Spacing, 0f, MaxBend, MinLength, out _));
        }
    }
}
