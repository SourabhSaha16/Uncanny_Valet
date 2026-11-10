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
        public void FacingTowardTurnsAPatronToTheCurbZone()
        {
            var slot = new Vector2(10f, 4f);
            Assert.AreEqual(WalkFacing.Left, WalkAnimMath.FacingToward(slot, new Vector2(8f, 4f), WalkFacing.Down));
            Assert.AreEqual(WalkFacing.Up, WalkAnimMath.FacingToward(slot, new Vector2(9.8f, 8f), WalkFacing.Right));
            Assert.AreEqual(WalkFacing.Right, WalkAnimMath.FacingToward(slot, slot, WalkFacing.Right));
        }

        [Test]
        public void CurbLookPoint_FacesAcrossToTheZoneHoweverFarUpOrDownItsCentreIs()
        {
            var centre = new Vector2(12f, 6f);
            var west = new Vector2(9f, -3f);
            var east = new Vector2(15f, 14f);
            Assert.AreEqual(WalkFacing.Right, WalkAnimMath.FacingToward(west, WalkAnimMath.CurbLookPoint(west, centre), WalkFacing.Down));
            Assert.AreEqual(WalkFacing.Left, WalkAnimMath.FacingToward(east, WalkAnimMath.CurbLookPoint(east, centre), WalkFacing.Up));
            // the old centre-point rule turned this patron to face up along the curb
            Assert.AreEqual(WalkFacing.Up, WalkAnimMath.FacingToward(west, centre, WalkFacing.Down));
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
