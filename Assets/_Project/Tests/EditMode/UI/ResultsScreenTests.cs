using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI;
using UncannyValet.UI.Screens;

namespace UncannyValet.Tests.UI
{
    public class ResultsScreenTests
    {
        private static ShiftResult Result(int blessings, int threshold, int curses = 0, int peakFavour = 1)
            => new ShiftResult(blessings, threshold, blessings >= threshold, curses, peakFavour);

        [Test]
        public void Build_ShowsBlessingsThresholdVerdictAndStatLines()
        {
            var content = ResultsFormatter.Build(Result(32, 30, 1, 3), 7);
            Assert.AreEqual("32 / 30", content.Blessings);
            Assert.AreEqual(ResultsFormatter.ClearedLabel, content.Verdict);
            Assert.IsTrue(content.Cleared);
            Assert.AreEqual("PEAK FAVOUR", content.Stats[1].Label);
            Assert.AreEqual("CURSES", content.Stats[2].Label);
            Assert.AreEqual(new[] { "7", "X3", "1" },
                new[] { content.Stats[0].Value, content.Stats[1].Value, content.Stats[2].Value });
        }

        [Test]
        public void Build_MissedThreshold_IsFailedAndNamesWhatWasMissing()
        {
            var content = ResultsFormatter.Build(Result(21, 30), 0);
            Assert.AreEqual(ResultsFormatter.FailedLabel, content.Verdict);
            Assert.IsFalse(content.Cleared);
            Assert.AreEqual("9 MORE BLESSINGS NEEDED", content.Goal);
        }

        [Test]
        public void Build_ExactlyTheThreshold_IsCleared()
        {
            var content = ResultsFormatter.Build(Result(30, 30), 0);
            Assert.AreEqual(ResultsFormatter.ClearedLabel, content.Verdict);
            Assert.AreEqual(ResultsFormatter.ReachedLine, content.Goal);
        }

        [Test]
        public void ShiftStats_CountsParkedCarsOnce_ThenResets()
        {
            var bus = new EventBus();
            using (var stats = new ShiftStats(bus))
            {
                bus.Publish(new CarParked(new CarId(1), 2));
                bus.Publish(new CarParked(new CarId(1), 3));
                bus.Publish(new CarParked(new CarId(2), 1));
                Assert.AreEqual(2, stats.CarsParked);
                bus.Publish(new ShiftStarted());
                Assert.AreEqual(0, stats.CarsParked);
            }
        }
    }
}
