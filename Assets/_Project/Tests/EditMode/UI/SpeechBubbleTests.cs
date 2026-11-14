using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Watchers;
using UncannyValet.Tests.Watchers;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Speech;
using UnityEngine;

namespace UncannyValet.Tests.UI
{
    public sealed class SpeechBubbleTests
    {
        private WatcherTestKit _kit;
        private SpeechBubbleTracker _tracker;
        private float _now;

        [SetUp]
        public void SetUp()
        {
            _kit = new WatcherTestKit();
            _now = 10f;
            _tracker = new SpeechBubbleTracker(_kit.Bus, () => _now);
        }

        [TearDown]
        public void TearDown() => _tracker.Dispose();

        private void Show(string text, float seconds = 4f)
            => _kit.Bus.Publish(new SpeechShown(new SpeechRequest("MRS. OKAFOR", text, Vector2.zero, null, SpeechPriority.Tip), seconds));

        [Test]
        public void NothingShown_NoBubble() => Assert.IsFalse(_tracker.TryGetCurrent(_now, out _));

        [Test]
        public void ABubble_StaysItsSeconds()
        {
            Show("HELLO");
            Assert.IsTrue(_tracker.TryGetCurrent(13.9f, out var request));
            Assert.AreEqual("HELLO", request.Text);
            Assert.IsFalse(_tracker.TryGetCurrent(14f, out _));
        }

        [Test]
        public void ANewBubble_ReplacesTheOldOne()
        {
            Show("FIRST");
            Show("SECOND");
            _tracker.TryGetCurrent(_now, out var request);
            Assert.AreEqual("SECOND", request.Text);
        }

        [Test]
        public void ALongLine_WrapsIntoTwoLinesWithinTheLimit()
        {
            Show("QUEUE'S FULL. CLEAR IT, I'M WATCHING.");
            Assert.AreEqual(2, _tracker.Lines.Count);
            foreach (var span in _tracker.Lines) Assert.LessOrEqual(span.Length, SpeechBubbleLayout.LineChars);
        }

        [Test]
        public void AShortLine_IsOneLine()
        {
            Show("TICK TOCK, VALET.");
            Assert.AreEqual(1, _tracker.Lines.Count);
        }

        [Test]
        public void Layout_FitsTheTextAndStaysOnTheCanvas()
        {
            var width = SpeechBubbleLayout.Width(11, SpeechBubbleLayout.LineChars);
            Assert.AreEqual(PixelTextLayout.Width(SpeechBubbleLayout.LineChars, 1) + SpeechBubbleLayout.Padding * 2, width);
            Assert.Less(width, PixelGrid.Width);
            Assert.AreEqual(0, SpeechBubbleLayout.Clamp(-20, width, PixelGrid.Width));
            Assert.AreEqual(PixelGrid.Width - width, SpeechBubbleLayout.Clamp(PixelGrid.Width, width, PixelGrid.Width));
            Assert.AreEqual(50, SpeechBubbleLayout.Clamp(50, width, PixelGrid.Width));
        }

        [Test]
        public void Layout_TwoLinesAreTaller()
            => Assert.Greater(SpeechBubbleLayout.Height(2), SpeechBubbleLayout.Height(1));
    }
}
