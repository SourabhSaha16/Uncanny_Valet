using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Tests.Pedestrians
{
    public class PedestrianBlockRuleTests
    {
        private const float Step = 0.5f;

        private static PedestrianBlockRule Rule(float wait = 2f, float pass = 1f)
        {
            var rule = new PedestrianBlockRule();
            rule.Configure(wait, pass);
            return rule;
        }

        [Test]
        public void ClearPathKeepsMoving()
        {
            Assert.AreEqual(PedestrianBlockState.Moving, Rule().Evaluate(Step, false));
        }

        [Test]
        public void BlockedPathWaitsThenPassesThrough()
        {
            var rule = Rule();
            for (var i = 0; i < 3; i++) Assert.AreEqual(PedestrianBlockState.Waiting, rule.Evaluate(Step, true));
            Assert.AreEqual(PedestrianBlockState.Passing, rule.Evaluate(Step, true));
        }

        [Test]
        public void ClearingWhileWaitingResumesAndForgetsTheWait()
        {
            var rule = Rule();
            rule.Evaluate(1.5f, true);
            Assert.AreEqual(PedestrianBlockState.Moving, rule.Evaluate(Step, false));
            Assert.AreEqual(PedestrianBlockState.Waiting, rule.Evaluate(1.5f, true));
        }

        [Test]
        public void PassingIgnoresBlockersThenWaitsAgain()
        {
            var rule = Rule();
            rule.Evaluate(2f, true);
            Assert.AreEqual(PedestrianBlockState.Passing, rule.State);
            Assert.AreEqual(PedestrianBlockState.Passing, rule.Evaluate(Step, true));
            Assert.AreEqual(PedestrianBlockState.Waiting, rule.Evaluate(Step, true));
        }

        [Test]
        public void NeverStaysStuckWhileAlwaysBlocked()
        {
            var rule = Rule(1.5f, 1f);
            var movedSeconds = 0f;
            for (var t = 0f; t < 60f; t += 0.02f)
                if (rule.Evaluate(0.02f, true) != PedestrianBlockState.Waiting) movedSeconds += 0.02f;
            Assert.Greater(movedSeconds, 20f);
        }

        [Test]
        public void ZeroPassTimeJustMovesOnAfterTheWait()
        {
            var rule = Rule(1f, 0f);
            Assert.AreEqual(PedestrianBlockState.Moving, rule.Evaluate(1f, true));
        }

        [Test]
        public void ResetStartsOver()
        {
            var rule = Rule();
            rule.Evaluate(1.5f, true);
            rule.Reset();
            Assert.AreEqual(PedestrianBlockState.Waiting, rule.Evaluate(1.5f, true));
        }

        [Test]
        public void BounceStrengthIgnoresNudgesAndCapsAtOne()
        {
            Assert.AreEqual(0f, PedestrianBounceRule.Strength(0.2f, 0.4f, 5f));
            Assert.AreEqual(0.5f, PedestrianBounceRule.Strength(2.5f, 0.4f, 5f), 1e-5f);
            Assert.AreEqual(1f, PedestrianBounceRule.Strength(20f, 0.4f, 5f));
        }

        [Test]
        public void BounceGoesAwayPeaksSmallAndSettlesBackToZero()
        {
            var bounce = new PedestrianBounceRule();
            bounce.Configure(0.2f, 0.25f, 0.6f);
            Assert.IsTrue(bounce.TryStart(new Vector2(3f, 0f), 1f));
            var peak = 0f;
            for (var i = 0; i < 5; i++)
            {
                bounce.Step(0.05f);
                if (!bounce.Active) break;
                Assert.GreaterOrEqual(bounce.Offset.x, 0f);
                Assert.AreEqual(0f, bounce.Offset.y, 1e-5f);
                peak = Mathf.Max(peak, bounce.Offset.x);
            }
            Assert.LessOrEqual(peak, 0.2f + 1e-4f);
            Assert.Greater(peak, 0.1f);
            Assert.IsFalse(bounce.Active);
            Assert.AreEqual(Vector2.zero, bounce.Offset);
        }

        [Test]
        public void BounceCannotRetriggerUntilTheCooldownPasses()
        {
            var bounce = new PedestrianBounceRule();
            bounce.Configure(0.2f, 0.25f, 0.6f);
            Assert.IsTrue(bounce.TryStart(Vector2.right, 1f));
            Assert.IsFalse(bounce.TryStart(Vector2.right, 1f));
            for (var i = 0; i < 10; i++) bounce.Step(0.05f);
            Assert.IsFalse(bounce.Active);
            Assert.IsFalse(bounce.TryStart(Vector2.right, 1f));
            for (var i = 0; i < 10; i++) bounce.Step(0.05f);
            Assert.IsTrue(bounce.TryStart(Vector2.right, 1f));
        }

        [Test]
        public void BounceRefusesZeroStrengthOrNoDirection()
        {
            var bounce = new PedestrianBounceRule();
            Assert.IsFalse(bounce.TryStart(Vector2.right, 0f));
            Assert.IsFalse(bounce.TryStart(Vector2.zero, 1f));
        }

        [Test]
        public void BounceShapeStartsAndEndsAtZero()
        {
            Assert.AreEqual(0f, PedestrianBounceRule.Shape(0f), 1e-5f);
            Assert.AreEqual(0f, PedestrianBounceRule.Shape(1f), 1e-5f);
            Assert.Greater(PedestrianBounceRule.Shape(0.3f), PedestrianBounceRule.Shape(0.7f));
        }

        [Test]
        public void RemainingDistanceShrinksAsTheWalkerAdvances()
        {
            var path = new PathFollower();
            path.Begin(new List<Vector2> { Vector2.zero, new Vector2(4f, 0f), new Vector2(4f, 3f) });
            Assert.AreEqual(7f, path.Remaining, 1e-4f);
            path.Advance(5f);
            Assert.AreEqual(2f, path.Remaining, 1e-4f);
            path.Advance(5f);
            Assert.AreEqual(0f, path.Remaining, 1e-4f);
        }
    }
}
