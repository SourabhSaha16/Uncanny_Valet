using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Watchers;
using UncannyValet.Watchers;
using UnityEngine;

namespace UncannyValet.Tests.Watchers
{
    /// <summary>The speech director on the bus: any speaker can ask, one bubble at a time comes out. This is also the path floating gag lines use.</summary>
    public sealed class SpeechDirectorTests
    {
        private WatcherTestKit _kit;
        private SpeechDirector _director;

        [SetUp]
        public void SetUp()
        {
            _kit = new WatcherTestKit();
            _director = new SpeechDirector(_kit.Bus, new SpeechScheduler(4f, 8f));
        }

        [TearDown]
        public void TearDown() => _director.Dispose();

        private void Ask(string text, SpeechPriority priority)
            => _kit.Bus.Publish(new SpeechRequested(new SpeechRequest("PEDESTRIAN", text, Vector2.zero, null, priority)));

        [Test]
        public void AGagLineRequest_IsShownForTheBubbleSeconds()
        {
            Ask("NICE CAR", SpeechPriority.GagLine);
            _director.Tick(0f);
            Assert.AreEqual(1, _kit.Shown.Count);
            Assert.AreEqual(4f, _kit.Shown[0].Seconds);
            Assert.AreEqual("NICE CAR", _kit.Shown[0].Request.Text);
        }

        [Test]
        public void TwoRequestsInOneFrame_ShowOnlyTheHigherPriority()
        {
            Ask("GAG", SpeechPriority.GagLine);
            Ask("COMMENT", SpeechPriority.Comment);
            _director.Tick(0f);
            Assert.AreEqual(1, _kit.Shown.Count);
            Assert.AreEqual("COMMENT", _kit.Shown[0].Request.Text);
        }

        [Test]
        public void NothingNewShowsWithinEightSeconds()
        {
            Ask("A", SpeechPriority.Comment);
            _director.Tick(0f);
            Ask("B", SpeechPriority.Tip);
            _director.Tick(5f);
            Ask("C", SpeechPriority.Tip);
            _director.Tick(8.1f);
            Assert.AreEqual(2, _kit.Shown.Count);
            Assert.AreEqual("C", _kit.Shown[1].Request.Text);
        }

        [Test]
        public void ANewShift_ClearsTheLockout()
        {
            Ask("A", SpeechPriority.Comment);
            _director.Tick(0f);
            _kit.Bus.Publish(new ShiftStarted());
            Ask("B", SpeechPriority.Comment);
            _director.Tick(1f);
            Assert.AreEqual(2, _kit.Shown.Count);
        }

        [Test]
        public void AfterDispose_RequestsAreIgnored()
        {
            _director.Dispose();
            Ask("A", SpeechPriority.Comment);
            _director.Tick(0f);
            Assert.AreEqual(0, _kit.Shown.Count);
        }
    }
}
