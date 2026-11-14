using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Tests.Pedestrians
{
    /// <summary>Route choice on crazy lots (free direct routes) against training lots (crosswalks). Issue #124.</summary>
    public class WalkerRouteRuleTests
    {
        private static List<Vector2> Crosswalk() => new List<Vector2>
        {
            new Vector2(9f, 1f), new Vector2(9f, 3f), new Vector2(2f, 3f), new Vector2(2f, 5f), new Vector2(0f, 5f),
        };

        [Test]
        public void FreeRoutes_WalkTheStraightLineFromStartToDestination()
        {
            var route = Crosswalk();
            WalkerRouteRule.Choose(true, route);
            Assert.AreEqual(2, route.Count);
            Assert.AreEqual(new Vector2(9f, 1f), route[0]);
            Assert.AreEqual(new Vector2(0f, 5f), route[1]);
        }

        [Test]
        public void TrainingLots_KeepTheCrosswalkRouteUntouched()
        {
            var route = Crosswalk();
            WalkerRouteRule.Choose(false, route);
            CollectionAssert.AreEqual(Crosswalk(), route);
        }

        [Test]
        public void ShortRoutes_AreLeftAlone()
        {
            var route = new List<Vector2> { Vector2.zero, Vector2.one };
            WalkerRouteRule.Choose(true, route);
            Assert.AreEqual(2, route.Count);
            var single = new List<Vector2> { Vector2.zero };
            WalkerRouteRule.Choose(true, single);
            Assert.AreEqual(1, single.Count);
        }

        [Test]
        public void TheSwitchFollowsTheTuningValue_AndIsOffWithoutOne()
        {
            Assert.IsFalse(WalkerRouteRule.FreeRoutes(null));
            var tuning = new TuningStore(new EventBus());
            Assert.IsFalse(WalkerRouteRule.FreeRoutes(tuning), "unseeded means crosswalks");
            tuning.Seed(TuningKey.PedestrianFreeRoutes, 1f);
            Assert.IsTrue(WalkerRouteRule.FreeRoutes(tuning));
            tuning.Set(TuningKey.PedestrianFreeRoutes, 0f);
            Assert.IsFalse(WalkerRouteRule.FreeRoutes(tuning));
        }
    }
}
