using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;
using UncannyValet.Valet;
using UnityEngine;

namespace UncannyValet.Tests.Valet
{
    public class ThunderDashTests
    {
        private const float Dt = 0.02f;

        // Runs the rule as the body would: it moves exactly as asked, unless a wall stops it at wallAt units.
        private static float Run(DashRule rule, float wallAt, out int steps)
        {
            var position = 0f;
            var moved = 0f;
            steps = 0;
            while (rule.IsActive && steps < 100)
            {
                var velocity = rule.Step(Dt, moved);
                if (!rule.IsActive) break;
                var next = Mathf.Min(position + velocity.x * Dt, wallAt);
                moved = next - position;
                position = next;
                steps++;
            }
            return position;
        }

        [TestCase(1, 5f)]
        [TestCase(2, 8f)]
        public void TheDashCoversTheLevelsDistanceInTheDashTime(int level, float distance)
        {
            var values = PowerDefaults.Values(StoryPower.ThunderStride, level);
            Assert.AreEqual(distance, values.Distance);
            Assert.AreEqual(0.2f, values.Secondary, 0.0001f);
            var rule = new DashRule();
            rule.Begin(Vector2.right, values.Distance, values.Secondary);
            var reached = Run(rule, 100f, out var steps);
            Assert.AreEqual(distance, reached, 0.01f);
            Assert.LessOrEqual(steps * Dt, 0.2f + Dt);
            Assert.IsFalse(rule.IsActive);
        }

        [Test]
        public void ASolidThingStopsTheDashAtTheContact()
        {
            var rule = new DashRule();
            rule.Begin(Vector2.right, 5f, 0.2f);
            var reached = Run(rule, 2f, out var steps);
            Assert.AreEqual(2f, reached, 0.01f);
            Assert.Less(steps, 10, "it ended early, at the wall");
            Assert.IsFalse(rule.IsActive);
        }

        [Test]
        public void ADashWithNoRoomStartsAndEndsAtOnce()
        {
            var rule = new DashRule();
            rule.Begin(Vector2.right, 5f, 0.2f);
            var reached = Run(rule, 0f, out var steps);
            Assert.AreEqual(0f, reached, 0.0001f);
            Assert.LessOrEqual(steps, 2);
            Assert.IsFalse(rule.IsActive);
        }

        [Test]
        public void ZeroDirectionOrDistanceOrTimeNeverStarts()
        {
            var rule = new DashRule();
            rule.Begin(Vector2.zero, 5f, 0.2f);
            Assert.IsFalse(rule.IsActive);
            rule.Begin(Vector2.up, 0f, 0.2f);
            Assert.IsFalse(rule.IsActive);
            rule.Begin(Vector2.up, 5f, 0f);
            Assert.IsFalse(rule.IsActive);
            Assert.AreEqual(Vector2.zero, rule.Step(Dt, 0f));
        }

        [Test]
        public void TheDashGoesTheMovingWayElseTheFacing()
        {
            Assert.AreEqual(new Vector2(0.6f, 0.8f).x, DashDirection.Choose(new Vector2(3f, 4f), ValetFacing.Left).x, 0.0001f);
            Assert.AreEqual(1f, DashDirection.Choose(new Vector2(3f, 4f), ValetFacing.Left).magnitude, 0.0001f);
            Assert.AreEqual(Vector2.left, DashDirection.Choose(Vector2.zero, ValetFacing.Left));
            Assert.AreEqual(Vector2.right, DashDirection.Choose(Vector2.zero, ValetFacing.Right));
            Assert.AreEqual(Vector2.up, DashDirection.Choose(Vector2.zero, ValetFacing.Up));
            Assert.AreEqual(Vector2.down, DashDirection.Choose(Vector2.zero, ValetFacing.Down));
        }

        [Test]
        public void TheStreakIsJaggedSnappedAndFades()
        {
            var points = new Vector2[4];
            BoltShape.Layout(Vector2.zero, new Vector2(3f, 0f), 0.4f, new[] { 1f, -1f }, 16f, points);
            Assert.AreEqual(Vector2.zero, points[0]);
            Assert.AreEqual(new Vector2(3f, 0f), points[3]);
            Assert.AreEqual(0.4f, Mathf.Abs(points[1].y), 0.07f);
            Assert.AreEqual(-Mathf.Sign(points[1].y), Mathf.Sign(points[2].y));
            foreach (var p in points) Assert.AreEqual(0f, p.x * 16f - Mathf.Round(p.x * 16f), 0.0001f);
            Assert.AreEqual(1f, BoltShape.Fade(0f, 0.3f));
            Assert.AreEqual(0.5f, BoltShape.Fade(0.15f, 0.3f), 0.0001f);
            Assert.AreEqual(0f, BoltShape.Fade(0.4f, 0.3f));
        }
    }
}
