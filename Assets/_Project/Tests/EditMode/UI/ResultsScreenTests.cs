using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI;
using UncannyValet.UI.Screens;

namespace UncannyValet.Tests.UI
{
    public class ResultsScreenTests
    {
        private static ShiftResult Result(int score, int stars, int complaints = 0)
            => new ShiftResult(score, stars, complaints, 150, 300, 450);

        [Test]
        public void Build_ShowsScoreStarsAndStatLines()
        {
            var content = ResultsFormatter.Build(Result(320, 2, 1), 7, 3);
            Assert.AreEqual("$320", content.Score);
            Assert.AreEqual(2, content.Stars);
            Assert.AreEqual(new[] { "7", "X3", "1" },
                new[] { content.Stats[0].Value, content.Stats[1].Value, content.Stats[2].Value });
        }

        [Test]
        public void Build_NamesTheNextStarTarget()
        {
            Assert.AreEqual("NEXT STAR AT $150 (50 TO GO)", ResultsFormatter.Build(Result(100, 0), 0, 1).Goal);
            Assert.AreEqual("NEXT STAR AT $450 (130 TO GO)", ResultsFormatter.Build(Result(320, 2), 0, 1).Goal);
        }

        [Test]
        public void Build_ThreeStars_HasNoNextTarget()
            => Assert.AreEqual(ResultsFormatter.TopMarks, ResultsFormatter.Build(Result(500, 3), 0, 1).Goal);

        [Test]
        public void StarStamp_StartsBigAndSlamsToNormalSize()
        {
            Assert.AreEqual(0f, StarStamp.Scale(0, 0.1f));
            Assert.AreEqual(StarStamp.StartScale, StarStamp.Scale(0, StarStamp.FirstDelaySeconds), 0.001f);
            Assert.AreEqual(1f, StarStamp.Scale(0, StarStamp.LandTime(0)), 0.001f);
            Assert.AreEqual(1f, StarStamp.Scale(0, 99f), 0.001f);
        }

        [Test]
        public void StarStamp_StarsLandOneAfterAnother()
        {
            Assert.Less(StarStamp.LandTime(0), StarStamp.LandTime(1));
            Assert.Less(StarStamp.LandTime(1), StarStamp.LandTime(2));
            Assert.AreEqual(0f, StarStamp.Scale(1, StarStamp.FirstDelaySeconds));
        }

        [Test]
        public void StarStamp_OnlyTheThirdStarBursts_OnlyWhenEarned()
        {
            var landed = StarStamp.LandTime(StarStamp.BurstStar);
            Assert.AreEqual(-1f, StarStamp.BurstProgress(2, 3, landed - 0.01f));
            Assert.AreEqual(0.5f, StarStamp.BurstProgress(2, 3, landed + StarStamp.BurstSeconds * 0.5f), 0.001f);
            Assert.AreEqual(-1f, StarStamp.BurstProgress(2, 3, landed + StarStamp.BurstSeconds + 0.01f));
            Assert.AreEqual(-1f, StarStamp.BurstProgress(2, 2, landed + 0.1f));
            Assert.AreEqual(-1f, StarStamp.BurstProgress(1, 3, landed + 0.1f));
        }

        [Test]
        public void ShiftStats_CountsParkedCarsOnceAndBestCombo_ThenResets()
        {
            var bus = new EventBus();
            using (var stats = new ShiftStats(bus))
            {
                bus.Publish(new CarParked(new CarId(1), 2));
                bus.Publish(new CarParked(new CarId(1), 3));
                bus.Publish(new CarParked(new CarId(2), 1));
                bus.Publish(new ScoreChanged(50, 3, 0, 0));
                bus.Publish(new ScoreChanged(60, 1, 1, 0));
                Assert.AreEqual(2, stats.CarsParked);
                Assert.AreEqual(3, stats.BestCombo);
                bus.Publish(new ShiftStarted());
                Assert.AreEqual(0, stats.CarsParked);
                Assert.AreEqual(1, stats.BestCombo);
            }
        }
    }
}
