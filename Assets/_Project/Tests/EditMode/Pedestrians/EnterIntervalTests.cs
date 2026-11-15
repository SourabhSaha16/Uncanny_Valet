using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Tests.Pedestrians
{
    public class EnterIntervalTests
    {
        private static readonly Vector2 Range = new Vector2(10f, 40f);

        [Test]
        public void StaysInsideTheRangeWhateverTheOrder()
        {
            for (var i = 1; i < 200; i++)
            {
                var roll = PedestrianPlan.Hash01(i);
                Assert.That(EnterInterval.Next(Range, roll), Is.InRange(10f, 40f));
                Assert.That(EnterInterval.Next(new Vector2(40f, 10f), roll), Is.InRange(10f, 40f));
            }
        }

        [Test]
        public void IntervalsVaryBetweenDraws()
        {
            var seen = new HashSet<int>();
            for (var i = 1; i <= 30; i++) seen.Add(Mathf.RoundToInt(EnterInterval.Next(Range, PedestrianPlan.Hash01(i))));
            Assert.GreaterOrEqual(seen.Count, 15);
        }

        [Test]
        public void WalkersDoNotAllChooseTheDoorTogether()
        {
            var brains = new PedestrianBrain[8];
            var serial = 0;
            for (var i = 0; i < brains.Length; i++)
            {
                brains[i] = new PedestrianBrain();
                brains[i].BeginAmbient(5f, EnterInterval.Next(Range, PedestrianPlan.Hash01(++serial)));
            }
            var firstDue = -1;
            var allDue = -1;
            for (var second = 1; second <= 60 && allDue < 0; second++)
            {
                var due = 0;
                foreach (var brain in brains)
                {
                    brain.Tick(1f);
                    if (brain.EnterDue) due++;
                }
                if (due > 0 && firstDue < 0) firstDue = second;
                if (due == brains.Length) allDue = second;
            }
            Assert.Greater(allDue - firstDue, 5, "the first walker is due at " + firstDue + " s and the last at " + allDue + " s");
        }
    }
}
