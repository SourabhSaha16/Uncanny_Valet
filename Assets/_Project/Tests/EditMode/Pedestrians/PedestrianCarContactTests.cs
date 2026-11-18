using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Tests.Pedestrians
{
    /// <summary>Issue #192: only the driven car may knock or bounce a person; people plan and walk clear of the other cars.</summary>
    public class PedestrianCarContactTests
    {
        private sealed class BlockEverything : ILegBlocker
        {
            public bool Crosses(IReadOnlyList<Vector2> walk) => true;
        }

        private static readonly List<Vector2> Route = new List<Vector2> { Vector2.zero, new Vector2(10f, 0f), new Vector2(10f, 6f) };

        [Test]
        public void ADrivenCarMayKnock_EveryOtherCarMayNot()
        {
            Assert.IsTrue(PedestrianHitRule.MayKnock(true));
            Assert.IsFalse(PedestrianHitRule.MayKnock(false), "parked, queued, arriving, leaving and ambient cars never knock");
        }

        [Test]
        public void AWalkerWalkingIntoAStandingCarIsNotAHit()
        {
            Assert.AreEqual(0f, PedestrianHitRule.HitSpeed(1.5f, 1.5f), 1e-5f, "the walker's own speed is not the car's");
            Assert.AreEqual(3f, PedestrianHitRule.HitSpeed(5f, 2f), 1e-5f);
            Assert.AreEqual(4f, PedestrianHitRule.HitSpeed(4f, 0f), 1e-5f, "a standing person takes the full hit");
            Assert.AreEqual(0f, PedestrianHitRule.HitSpeed(1f, 6f), 1e-5f);
            Assert.AreEqual(3f, PedestrianHitRule.HitSpeed(3f, -1f), 1e-5f);
        }

        [Test]
        public void TheColliderNeverTurnsSolidInsideACar()
        {
            Assert.IsFalse(WalkerSolidRule.Resolve(true, true), "regression: passing ended inside a car and the body went solid there");
            Assert.IsTrue(WalkerSolidRule.Resolve(true, false));
            Assert.IsFalse(WalkerSolidRule.Resolve(false, false));
        }

        [Test]
        public void AWanderLegThatCrossesAnyBlockerIsNeverPicked()
        {
            var blockers = new LegBlockers();
            blockers.Add(new KeepOutAreas());
            blockers.Add(new BlockEverything());
            Assert.IsTrue(blockers.Crosses(Route));
            var picker = new WanderPicker(0.05f, 8, blockers, 3);
            Assert.IsFalse(picker.TryPick(Route, 0.5f, out _), "regression: roam legs ran through bays because nothing covered them");
        }

        [Test]
        public void NoBlockersMeansALegIsFree()
        {
            Assert.IsFalse(new LegBlockers().Crosses(Route));
        }

        [Test]
        public void ThePointAheadFollowsTheRouteRoundACorner()
        {
            var walker = new PathFollower();
            walker.Begin(Route);
            walker.Advance(9f);
            var ahead = walker.PointAhead(3f);
            Assert.AreEqual(10f, ahead.x, 1e-4f);
            Assert.AreEqual(2f, ahead.y, 1e-4f, "one unit to the corner, two more round it");
            Assert.AreEqual(new Vector2(10f, 6f), walker.PointAhead(100f));
        }

        [Test]
        public void ThePointAheadOfAStandingWalkerIsItsOwnPosition()
        {
            var walker = new PathFollower();
            walker.Begin(new List<Vector2> { Vector2.one });
            Assert.AreEqual(Vector2.one, walker.PointAhead(2f));
        }
    }
}
