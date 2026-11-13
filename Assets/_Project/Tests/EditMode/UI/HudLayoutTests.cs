using NUnit.Framework;
using UncannyValet.UI.Hud;
using UncannyValet.UI.Kit;

namespace UncannyValet.Tests.UI
{
    public class HudLayoutTests
    {
        private const int Border = PixelKit.PanelBorder;

        [Test]
        public void BoltAndCount_FitInTheFirstRowAboveTheBar()
        {
            var bolt = PixelIconData.Rows(PixelIcon.Bolt);
            Assert.GreaterOrEqual(HudLayout.BoltX, Border);
            Assert.LessOrEqual(HudLayout.BoltX + bolt[0].Length, HudLayout.CountX);
            Assert.LessOrEqual(HudLayout.BoltY + bolt.Length, HudLayout.BarY - HudLayout.MarkerOverhang);
            var fiveDigits = PixelTextLayout.Width("99999".Length, HudLayout.TextScale);
            Assert.LessOrEqual(HudLayout.CountX + fiveDigits, HudLayout.BlessingsWidth - Border);
        }

        [Test]
        public void BarMarkerAndGoalText_FitInsideThePanelBorder()
        {
            Assert.GreaterOrEqual(HudLayout.BarX - 1, Border);
            Assert.LessOrEqual(HudLayout.BarX + HudLayout.BarWidth + 1, HudLayout.BlessingsWidth - Border);
            Assert.GreaterOrEqual(HudLayout.BarY - HudLayout.MarkerOverhang, Border);
            Assert.Less(HudLayout.BarY + HudLayout.BarHeight + HudLayout.MarkerOverhang, HudLayout.GoalTextY);
            var text = PixelTextLayout.Width("999 / 999".Length, HudLayout.TextScale);
            Assert.LessOrEqual(HudLayout.BarX + text, HudLayout.BlessingsWidth - Border);
            Assert.LessOrEqual(HudLayout.GoalTextY + PixelTextLayout.Height(HudLayout.TextScale), HudLayout.BlessingsHeight - Border);
        }

        [Test]
        public void FavourBadge_HoldsTwoDigitMultiplierText()
        {
            var width = PixelTextLayout.Width("X4".Length, HudLayout.TextScale);
            Assert.LessOrEqual(width, HudLayout.FavourWidth - Border * 2);
            var bottom = HudLayout.FavourTextY + PixelTextLayout.Height(HudLayout.TextScale);
            Assert.GreaterOrEqual(HudLayout.FavourTextY, Border - 1);
            Assert.LessOrEqual(bottom, HudLayout.FavourHeight - Border + 1);
        }

        [Test]
        public void FavourWobble_StaysInsideTheBadgeMargin()
        {
            Assert.LessOrEqual(FavourWobble.AmplitudePixels, HudLayout.Margin);
        }

        [Test]
        public void BoltIcon_IsARectangularBitmapOfKnownColours()
        {
            var rows = PixelIconData.Rows(PixelIcon.Bolt);
            foreach (var row in rows)
            {
                Assert.AreEqual(rows[0].Length, row.Length);
                foreach (var c in row) Assert.IsTrue(c == '.' || PixelIconData.Keys.IndexOf(c) >= 0, "colour key " + c);
            }
        }
    }
}
