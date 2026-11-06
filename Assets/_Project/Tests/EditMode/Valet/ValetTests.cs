using NUnit.Framework;
using UncannyValet.Valet;
using UnityEngine;

namespace UncannyValet.Tests.Valet
{
    public class ValetTests
    {
        [Test]
        public void State_StartsOnFoot_AndTogglesBothWays()
        {
            var s = new ValetState();
            Assert.AreEqual(ValetMode.OnFoot, s.Mode);
            Assert.IsTrue(s.TryEnterCar());
            Assert.IsTrue(s.IsDriving);
            Assert.IsTrue(s.TryExitCar());
            Assert.AreEqual(ValetMode.OnFoot, s.Mode);
        }

        [Test]
        public void State_RejectsRepeatedTransitions_AndRaisesChangedOnce()
        {
            var s = new ValetState();
            var count = 0;
            s.Changed += _ => count++;
            Assert.IsFalse(s.TryExitCar());
            s.TryEnterCar();
            Assert.IsFalse(s.TryEnterCar());
            Assert.AreEqual(1, count);
        }

        [Test]
        public void FrameGuard_BlocksOnlyTheBlockedFrame()
        {
            var g = new FrameGuard();
            Assert.IsFalse(g.IsBlocked(0));
            g.Block(5);
            Assert.IsTrue(g.IsBlocked(5));
            Assert.IsFalse(g.IsBlocked(6));
        }

        [Test]
        public void ExitPlacement_NudgesAlongNormalizedAside()
        {
            var p = ExitPlacement.Beside(new Vector2(1, 1), new Vector2(0, 5), 2f);
            Assert.AreEqual(1f, p.x, 0.001f);
            Assert.AreEqual(3f, p.y, 0.001f);
        }

        [Test]
        public void ExitPlacement_ZeroAside_FallsBackToRight()
        {
            var p = ExitPlacement.Beside(Vector2.zero, Vector2.zero, 1.5f);
            Assert.AreEqual(1.5f, p.x, 0.001f);
            Assert.AreEqual(0f, p.y, 0.001f);
        }

        [Test]
        public void Picker_ChoosesNearest()
        {
            var p = new[] { new Vector2(3, 0), new Vector2(1, 0), new Vector2(2, 0) };
            Assert.AreEqual(1, NearestPicker.Pick(Vector2.zero, p, 5f));
        }

        [Test]
        public void Picker_ReturnsMinusOne_WhenOutOfRangeOrEmpty()
        {
            Assert.AreEqual(-1, NearestPicker.Pick(Vector2.zero, new[] { new Vector2(9, 0) }, 2f));
            Assert.AreEqual(-1, NearestPicker.Pick(Vector2.zero, new Vector2[0], 2f));
        }

        [Test]
        public void Picker_SkipsIneligible()
        {
            var p = new[] { new Vector2(1, 0), new Vector2(2, 0) };
            Assert.AreEqual(1, NearestPicker.Pick(Vector2.zero, p, 5f, i => i != 0));
        }
    }
}
