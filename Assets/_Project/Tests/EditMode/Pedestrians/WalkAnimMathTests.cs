using NUnit.Framework;
using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Tests.Pedestrians
{
    public class WalkAnimMathTests
    {
        [Test]
        public void FacingFollowsTheDominantAxis()
        {
            Assert.AreEqual(WalkFacing.Right, WalkAnimMath.Facing(new Vector2(1f, 0.2f), WalkFacing.Down));
            Assert.AreEqual(WalkFacing.Left, WalkAnimMath.Facing(new Vector2(-1f, 0.2f), WalkFacing.Down));
            Assert.AreEqual(WalkFacing.Up, WalkAnimMath.Facing(new Vector2(0.1f, 1f), WalkFacing.Down));
            Assert.AreEqual(WalkFacing.Down, WalkAnimMath.Facing(new Vector2(0.1f, -1f), WalkFacing.Up));
        }

        [Test]
        public void ZeroOrDiagonalHeadingKeepsTheCurrentFacing()
        {
            Assert.AreEqual(WalkFacing.Left, WalkAnimMath.Facing(Vector2.zero, WalkFacing.Left));
            Assert.AreEqual(WalkFacing.Up, WalkAnimMath.Facing(new Vector2(1f, 1f), WalkFacing.Up));
        }

        [Test]
        public void FrameCyclesThroughTheGroup()
        {
            Assert.AreEqual(0, WalkAnimMath.Frame(0f, 10f, 6));
            Assert.AreEqual(1, WalkAnimMath.Frame(0.11f, 10f, 6));
            Assert.AreEqual(5, WalkAnimMath.Frame(0.55f, 10f, 6));
            Assert.AreEqual(0, WalkAnimMath.Frame(0.65f, 10f, 6));
            Assert.AreEqual(0, WalkAnimMath.Frame(1f, 10f, 0));
        }
    }
}
