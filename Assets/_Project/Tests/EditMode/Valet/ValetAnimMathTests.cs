using NUnit.Framework;
using UncannyValet.Valet;
using UnityEngine;

namespace UncannyValet.Tests.Valet
{
    public class ValetAnimMathTests
    {
        private const float MinSpeed = 0.1f;

        [TestCase(1f, 0f, ValetFacing.Right)]
        [TestCase(-1f, 0.2f, ValetFacing.Left)]
        [TestCase(0.2f, 1f, ValetFacing.Up)]
        [TestCase(0f, -1f, ValetFacing.Down)]
        public void FacingFollowsTheDominantAxis(float x, float y, ValetFacing expected)
        {
            Assert.AreEqual(expected, ValetAnimMath.Facing(new Vector2(x, y), ValetFacing.Down, MinSpeed));
        }

        [Test]
        public void StandingStillKeepsTheFacing()
        {
            Assert.AreEqual(ValetFacing.Left, ValetAnimMath.Facing(Vector2.zero, ValetFacing.Left, MinSpeed));
            Assert.AreEqual(ValetFacing.Up, ValetAnimMath.Facing(new Vector2(0.05f, 0f), ValetFacing.Up, MinSpeed));
        }

        [Test]
        public void ExactDiagonalKeepsTheFacing()
        {
            Assert.AreEqual(ValetFacing.Right, ValetAnimMath.Facing(new Vector2(1f, 1f), ValetFacing.Right, MinSpeed));
        }

        [Test]
        public void FrameCyclesThroughTheGroup()
        {
            Assert.AreEqual(0, ValetAnimMath.Frame(0f, 10f, 6));
            Assert.AreEqual(1, ValetAnimMath.Frame(0.1f, 10f, 6));
            Assert.AreEqual(5, ValetAnimMath.Frame(0.55f, 10f, 6));
            Assert.AreEqual(0, ValetAnimMath.Frame(0.6f, 10f, 6));
            Assert.AreEqual(2, ValetAnimMath.Frame(1.45f, 10f, 6));
        }

        [Test]
        public void FrameIsZeroWithoutFrames()
        {
            Assert.AreEqual(0, ValetAnimMath.Frame(1f, 10f, 0));
        }

        [Test]
        public void ColumnAddsTheFrameToTheGroupStart()
        {
            Assert.AreEqual(14, ValetAnimMath.Column(12, 2));
        }
    }
}
