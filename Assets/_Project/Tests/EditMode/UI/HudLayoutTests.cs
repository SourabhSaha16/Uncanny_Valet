using NUnit.Framework;
using UncannyValet.UI.Hud;
using UncannyValet.UI.Kit;

namespace UncannyValet.Tests.UI
{
    public class HudLayoutTests
    {
        private const int OldScoreWidth = 112;
        private const int OldScoreHeight = 40;
        private const int OldComboWidth = 40;
        private const int OldComboHeight = 24;

        [Test]
        public void ScorePanel_IsAboutHalfTheOldSize()
        {
            Assert.LessOrEqual(HudLayout.ScoreWidth, OldScoreWidth * 0.6f);
            Assert.LessOrEqual(HudLayout.ScoreHeight, OldScoreHeight * 0.75f);
            Assert.LessOrEqual(HudLayout.ComboWidth, OldComboWidth * 0.7f);
            Assert.LessOrEqual(HudLayout.ComboHeight, OldComboHeight * 0.7f);
        }

        [Test]
        public void StarBar_AndEveryStarIcon_FitInsideTheScorePanelBorder()
        {
            var bar = new StarBarLayout(0, 150, 300, 450, HudLayout.BarWidth);
            var starWidth = PixelIconData.Rows(PixelIcon.StarLit)[0].Length;
            var starHeight = PixelIconData.Rows(PixelIcon.StarLit).Length;
            for (var star = 0; star < StarBarLayout.Stars; star++)
            {
                var x = HudLayout.StarX(bar, star);
                Assert.GreaterOrEqual(x, PixelKit.PanelBorder, "star " + star);
                Assert.LessOrEqual(x + starWidth, HudLayout.ScoreWidth - PixelKit.PanelBorder, "star " + star);
            }
            var top = HudLayout.BarY + HudLayout.BarHeight / 2 - starHeight / 2;
            Assert.GreaterOrEqual(top, PixelKit.PanelBorder);
            Assert.LessOrEqual(top + starHeight, HudLayout.ScoreHeight - PixelKit.PanelBorder);
        }

        [Test]
        public void CoinAndScoreText_FitInTheFirstRowAboveTheBar()
        {
            var coin = PixelIconData.Rows(PixelIcon.Coin);
            Assert.GreaterOrEqual(HudLayout.CoinX, PixelKit.PanelBorder);
            Assert.LessOrEqual(HudLayout.CoinX + coin[0].Length, HudLayout.TipsX);
            Assert.LessOrEqual(HudLayout.CoinY + coin.Length, HudLayout.BarY);
            var fiveDigits = PixelTextLayout.Width("$9999".Length, HudLayout.TextScale);
            Assert.LessOrEqual(HudLayout.TipsX + fiveDigits, HudLayout.ScoreWidth - PixelKit.PanelBorder);
            Assert.LessOrEqual(HudLayout.TipsY + PixelTextLayout.Height(HudLayout.TextScale), HudLayout.BarY);
        }

        [Test]
        public void ComboBadge_HoldsTwoDigitMultiplierText()
        {
            var width = PixelTextLayout.Width("X4".Length, HudLayout.TextScale);
            Assert.LessOrEqual(width, HudLayout.ComboWidth - PixelKit.PanelBorder * 2);
            var bottom = HudLayout.ComboTextY + PixelTextLayout.Height(HudLayout.TextScale);
            Assert.GreaterOrEqual(HudLayout.ComboTextY, PixelKit.PanelBorder - 1);
            Assert.LessOrEqual(bottom, HudLayout.ComboHeight - PixelKit.PanelBorder + 1);
        }

        [Test]
        public void ComboWobble_StaysInsideTheBadgeMargin()
        {
            Assert.LessOrEqual(ComboWobble.AmplitudePixels, HudLayout.Margin);
        }
    }
}
