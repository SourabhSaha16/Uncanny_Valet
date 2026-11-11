using NUnit.Framework;
using UncannyValet.Traffic;
using UnityEngine;

namespace UncannyValet.Tests.Traffic
{
    public class AmbientDensityTests
    {
        private const float Dt = 0.1f;
        private const LaneSet All = LaneSet.East | LaneSet.Top | LaneSet.EastBack | LaneSet.TopBack;

        private static AmbientDensity Make(int seed = 1, int min = 3, int target = 4) =>
            new AmbientDensity(seed, 5, new Vector2(0.85f, 1.15f), new Vector2(1.5f, 4f), min, target);

        [Test]
        public void BelowTheMinimum_ACarIsDueAtOnce()
        {
            Assert.IsTrue(Make().Tick(Dt, 2, All, out _));
            Assert.IsTrue(Make().Tick(Dt, 0, All, out _));
        }

        [Test]
        public void BetweenMinimumAndTarget_ACarWaitsForARandomGap()
        {
            AmbientDensity density = Make();
            Assert.IsFalse(density.Tick(Dt, 3, All, out _), "the wait starts from the first tick");
            float waited = 0f;
            bool spawned = false;
            while (!spawned && waited < 10f)
            {
                spawned = density.Tick(Dt, 3, All, out _);
                waited += Dt;
            }
            Assert.IsTrue(spawned);
            Assert.That(waited, Is.InRange(1.4f, 4.3f));
        }

        [Test]
        public void CarsAboutToLeave_DoNotCountTowardTheMinimum()
        {
            Assert.IsTrue(Make().Tick(Dt, 3, 2, All, out _), "three on screen, one of them leaving: send the next now");
        }

        [Test]
        public void ReplacingLeavingCars_NeverGoesMoreThanOneOverTheTarget()
        {
            Assert.IsTrue(Make().Tick(Dt, 4, 2, All, out _));
            Assert.IsFalse(Make().Tick(Dt, 5, 2, All, out _));
        }

        [Test]
        public void AtTheTarget_NothingSpawns()
        {
            AmbientDensity density = Make();
            for (int i = 0; i < 1000; i++) Assert.IsFalse(density.Tick(Dt, 4, All, out _));
        }

        [Test]
        public void TargetZero_SwitchesAmbientTrafficOff()
        {
            AmbientDensity density = Make(1, 0, 0);
            for (int i = 0; i < 1000; i++) Assert.IsFalse(density.Tick(Dt, 0, All, out _));
        }

        [Test]
        public void TheTargetNeverFallsBelowTheMinimum()
        {
            AmbientDensity density = Make(1, 3, 4);
            density.SetTargetOnScreen(1);
            Assert.AreEqual(3, density.Target);
            density.SetMinOnScreen(6);
            Assert.AreEqual(6, density.Target);
        }

        [Test]
        public void NeverPicksALaneThatIsNotOpen()
        {
            AmbientDensity density = Make();
            LaneSet open = LaneSet.EastBack | LaneSet.Top;
            for (int i = 0; i < 500; i++)
            {
                Assert.IsTrue(density.Tick(Dt, 0, open, out SpawnRequest request));
                Assert.IsTrue(TrafficLanes.Has(open, request.Lane), request.Lane.ToString());
            }
        }

        [Test]
        public void NoOpenLane_NothingSpawnsAndTheCarStaysDue()
        {
            AmbientDensity density = Make();
            Assert.IsFalse(density.Tick(Dt, 0, LaneSet.None, out _));
            Assert.IsTrue(density.Tick(Dt, 0, LaneSet.East, out _), "as soon as a lane opens the car comes");
        }

        [Test]
        public void EveryOpenLaneGetsCars_SoBothDirectionsMix()
        {
            AmbientDensity density = Make();
            var seen = new bool[TrafficLanes.Count];
            for (int i = 0; i < 400; i++)
            {
                density.Tick(Dt, 0, All, out SpawnRequest request);
                seen[(int)request.Lane] = true;
            }
            CollectionAssert.AreEqual(new[] { true, true, true, true }, seen);
        }

        [Test]
        public void SpeedsStayInTheRangeAndLooksInTheList()
        {
            AmbientDensity density = Make();
            for (int i = 0; i < 400; i++)
            {
                density.Tick(Dt, 0, All, out SpawnRequest request);
                Assert.That(request.SpeedScale, Is.InRange(0.85f, 1.15f));
                Assert.That(request.LookIndex, Is.InRange(0, 4));
            }
        }

        [Test]
        public void SameSeed_SameSequence()
        {
            AmbientDensity a = Make(7);
            AmbientDensity b = Make(7);
            for (int i = 0; i < 200; i++)
            {
                bool ra = a.Tick(Dt, 3, All, out SpawnRequest qa);
                bool rb = b.Tick(Dt, 3, All, out SpawnRequest qb);
                Assert.AreEqual(ra, rb);
                if (ra) Assert.AreEqual((qa.Lane, qa.LookIndex, qa.SpeedScale), (qb.Lane, qb.LookIndex, qb.SpeedScale));
            }
        }
    }
}
