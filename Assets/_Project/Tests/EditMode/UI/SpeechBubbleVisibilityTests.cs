using NUnit.Framework;
using UncannyValet.UI.Speech;

namespace UncannyValet.Tests.UI
{
    public sealed class SpeechBubbleVisibilityTests
    {
        private const float Margin = 0.05f;

        [Test]
        public void CentreOfTheViewIsVisible() => Assert.IsTrue(SpeechBubbleVisibility.IsInView(0.5f, 0.5f, 10f, Margin));

        [Test]
        public void FarOffToTheSideIsHidden() => Assert.IsFalse(SpeechBubbleVisibility.IsInView(1.5f, 0.5f, 10f, Margin));

        [Test]
        public void BelowTheViewIsHidden() => Assert.IsFalse(SpeechBubbleVisibility.IsInView(0.5f, -0.2f, 10f, Margin));

        [Test]
        public void JustPastTheEdgeInsideTheMarginIsVisible() => Assert.IsTrue(SpeechBubbleVisibility.IsInView(1.03f, 0.5f, 10f, Margin));

        [Test]
        public void PastTheMarginIsHidden() => Assert.IsFalse(SpeechBubbleVisibility.IsInView(1.06f, 0.5f, 10f, Margin));

        [Test]
        public void BehindTheCameraIsHidden() => Assert.IsFalse(SpeechBubbleVisibility.IsInView(0.5f, 0.5f, -1f, Margin));

        [Test]
        public void ReturningToViewShowsAgain()
        {
            Assert.IsFalse(SpeechBubbleVisibility.IsInView(1.4f, 0.5f, 10f, Margin));
            Assert.IsTrue(SpeechBubbleVisibility.IsInView(0.9f, 0.5f, 10f, Margin));
        }
    }
}
