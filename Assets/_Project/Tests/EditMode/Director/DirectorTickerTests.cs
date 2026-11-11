using NUnit.Framework;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>The director collects world time only while the world runs, so freeze time sends no cars.</summary>
    public class DirectorTickerTests
    {
        [Test]
        public void TicksOnceTheIntervalIsCollected_ReportingTheElapsedTime()
        {
            var ticker = new DirectorTicker();
            Assert.IsFalse(ticker.TryTick(0.2f, false, 0.5f, out _));
            Assert.IsTrue(ticker.TryTick(0.4f, false, 0.5f, out var elapsed));
            Assert.AreEqual(0.6f, elapsed, 0.0001f);
            Assert.IsFalse(ticker.TryTick(0.1f, false, 0.5f, out _));
        }

        [Test]
        public void WhilePaused_NeverTicksAndCollectsNothing()
        {
            var ticker = new DirectorTicker();
            for (var i = 0; i < 100; i++) Assert.IsFalse(ticker.TryTick(1f, true, 0.5f, out _));
            Assert.IsFalse(ticker.TryTick(0.3f, false, 0.5f, out _), "the paused time did not count");
        }

        [Test]
        public void ResumingAfterAPause_ContinuesFromWhereItStopped()
        {
            var ticker = new DirectorTicker();
            Assert.IsFalse(ticker.TryTick(0.3f, false, 0.5f, out _));
            Assert.IsFalse(ticker.TryTick(5f, true, 0.5f, out _));
            Assert.IsTrue(ticker.TryTick(0.2f, false, 0.5f, out var elapsed));
            Assert.AreEqual(0.5f, elapsed, 0.0001f);
        }

        [Test]
        public void Reset_ForgetsCollectedTime()
        {
            var ticker = new DirectorTicker();
            ticker.TryTick(0.4f, false, 0.5f, out _);
            ticker.Reset();
            Assert.IsFalse(ticker.TryTick(0.2f, false, 0.5f, out _));
        }
    }
}
