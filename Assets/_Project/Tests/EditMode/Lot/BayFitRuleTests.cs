using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public sealed class BayFitRuleTests
    {
        private const float Tol = 0.05f;
        private readonly BayFitRule _rule = new BayFitRule(new Vector2(1f, 2f), Tol);

        private bool Inside(params Vector2[] pts) => _rule.IsInside(pts, pts.Length);

        [Test] public void CarWellInsideIsInside() => Assert.IsTrue(Inside(new Vector2(-0.5f, -1f), new Vector2(0.5f, 1f)));
        [Test] public void TouchingTheLineIsInside() => Assert.IsTrue(Inside(new Vector2(1f, 2f), new Vector2(-1f, -2f)));
        [Test] public void PastTheLineWithinToleranceIsInside() => Assert.IsTrue(Inside(new Vector2(1f + Tol, 0f)));
        [Test] public void JustPastToleranceIsOutside() => Assert.IsFalse(Inside(new Vector2(1f + Tol + 0.01f, 0f)));
        [Test] public void OutsideRight() => Assert.IsFalse(Inside(new Vector2(1.5f, 0f)));
        [Test] public void OutsideLeft() => Assert.IsFalse(Inside(new Vector2(-1.5f, 0f)));
        [Test] public void OutsideTop() => Assert.IsFalse(Inside(new Vector2(0f, 2.5f)));
        [Test] public void OutsideBottom() => Assert.IsFalse(Inside(new Vector2(0f, -2.5f)));
        [Test] public void OneCornerOutIsOutside() => Assert.IsFalse(Inside(new Vector2(0f, 0f), new Vector2(0.9f, 2.4f)));
        [Test] public void NoPointsIsNeverInside() => Assert.IsFalse(_rule.IsInside(new Vector2[0], 0));
    }
}
