using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.UI.Hud;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.Tests.UI
{
    public class ClockPlacementTests
    {
        private static readonly Color Green = new Color(0.2f, 0.8f, 0.3f, 1f);
        private const int GapMax = 20;
        private const int InsetMax = 40;

        [TestCase(0)]
        [TestCase(ClockSettings.DefaultGapPixels)]
        [TestCase(GapMax)]
        public void Clock_SitsBelowTheScorePanel(int gap)
        {
            var scoreBottom = HudLayout.Margin - 1 + HudLayout.BlessingsHeight;
            Assert.GreaterOrEqual(ClockLayout.Y(gap), scoreBottom);
        }

        [Test]
        public void Clock_IsFlushWithTheScorePanelAtTheLeftEdge()
        {
            Assert.AreEqual(HudLayout.Margin, ClockLayout.X(0));
            Assert.AreEqual(HudLayout.BlessingsWidth, ClockLayout.Width);
        }

        [TestCase(0, 0)]
        [TestCase(ClockSettings.DefaultGapPixels, 0)]
        [TestCase(GapMax, InsetMax)]
        public void Clock_StaysOnTheCanvasAndLeftOfTheCentreAndThePowerColumn(int gap, int inset)
        {
            var right = ClockLayout.X(inset) + ClockLayout.Width;
            var powerColumn = PixelGrid.Width - HudLayout.Margin - PowerBarPainter.ColumnWidth;
            Assert.Less(right, PixelGrid.Width / 2);
            Assert.Less(right, powerColumn);
            Assert.LessOrEqual(ClockLayout.Y(gap) + ClockLayout.Height, PixelGrid.Height);
        }

        [Test]
        public void ClockText_IsCentredInsideTheTab()
        {
            var textHeight = PixelTextLayout.Height(2);
            var top = ClockLayout.TextY(ClockSettings.DefaultGapPixels, textHeight);
            Assert.GreaterOrEqual(top, ClockLayout.Y(ClockSettings.DefaultGapPixels));
            Assert.LessOrEqual(top + textHeight, ClockLayout.Y(ClockSettings.DefaultGapPixels) + ClockLayout.Height);
        }

        [Test]
        public void PerfectPark_FillPopUsesTheSharedGreenUnpulled()
        {
            var color = PowerFillPopRule.ColorOf(PowerFillSource.PerfectPark, 1f, Green);
            Assert.AreEqual(Green, color);
        }

        [Test]
        public void ParkedInside_StaysDistinguishableFromPerfectPark()
        {
            var parked = PowerFillPopRule.ColorOf(PowerFillSource.ParkedInside, 0.5f, Green);
            Assert.AreNotEqual(Green, parked);
        }
    }
}
