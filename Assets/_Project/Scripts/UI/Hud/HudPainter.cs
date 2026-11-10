using System.Text;
using UncannyValet.Core.Contracts;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// The corner HUD from the style guide's Scene 10: shift clock top centre, tips and the three-star bar top left,
    /// combo badge top right. Strings are cached and rebuilt only when a number changes.
    /// </summary>
    public sealed class HudPainter
    {
        private const int Margin = HudLayout.Margin;
        private const int ClockWidth = 58;
        private const int ClockHeight = 22;
        private const int BigScale = 2;
        private const int MinComboShown = 2;
        private const float BlinkPeriod = 0.5f;
        private const float NeverRisen = -100f;

        private readonly PixelCanvas _canvas;
        private readonly IGameClock _clock;
        private readonly IShiftScoreView _score;
        private readonly StringBuilder _clockText = new StringBuilder(8);
        private readonly CachedText _tips = new CachedText("$");
        private readonly CachedText _combo = new CachedText("X");
        private int _clockSeconds = -1;
        private int _lastCombo = 1;
        private float _comboRisenAt = NeverRisen;

        public HudPainter(PixelCanvas canvas, IGameClock clock, IShiftScoreView score)
        {
            _canvas = canvas;
            _clock = clock;
            _score = score;
        }

        public void Draw(float time)
        {
            DrawClock(time);
            DrawScore();
            DrawCombo(time);
        }

        private void DrawClock(float time)
        {
            var seconds = ClockText.WholeSeconds(_clock.SecondsRemaining);
            if (seconds != _clockSeconds)
            {
                _clockSeconds = seconds;
                _clockText.Clear();
                ClockText.Write(_clockText, seconds);
            }
            var x = (PixelGrid.Width - ClockWidth) / 2;
            _canvas.Panel(x, Margin - 1, ClockWidth, ClockHeight, PixelPalette.PanelDark);
            var level = Alarm.ForClock(seconds);
            var flash = level == AlarmLevel.Danger && !Blink.IsOn(time, BlinkPeriod);
            var color = flash ? PixelPalette.Cream : AlarmColors.For(level, PixelPalette.Cream);
            var width = PixelTextLayout.Width(_clockText.Length, BigScale);
            _canvas.Text(_clockText, PixelTextLayout.Centered(x, ClockWidth, width), Margin + 4, color, BigScale);
        }

        private void DrawScore()
        {
            _tips.Set(_score.Score);
            var x = Margin;
            var y = Margin - 1;
            _canvas.Panel(x, y, HudLayout.ScoreWidth, HudLayout.ScoreHeight, PixelPalette.PanelDark);
            _canvas.Icon(PixelIcon.Coin, x + HudLayout.CoinX, y + HudLayout.CoinY, 1, Color.white);
            _canvas.Text(_tips.Text, x + HudLayout.TipsX, y + HudLayout.TipsY, PixelPalette.Gold, HudLayout.TextScale);
            DrawStarBar(x, y);
        }

        private void DrawStarBar(int panelX, int panelY)
        {
            var left = panelX + HudLayout.BarX;
            var top = panelY + HudLayout.BarY;
            var layout = new StarBarLayout(_score.Score, _score.OneStarScore, _score.TwoStarScore,
                _score.ThreeStarScore, HudLayout.BarWidth);
            _canvas.Fill(left - 1, top - 1, HudLayout.BarWidth + 2, HudLayout.BarHeight + 2, PixelPalette.Track);
            _canvas.Fill(left, top, layout.Fill, HudLayout.BarHeight, PixelPalette.Gold);
            for (var star = 0; star < StarBarLayout.Stars; star++)
            {
                var icon = layout.IsLit(star) ? PixelIcon.StarLit : PixelIcon.StarUnlit;
                var x = panelX + HudLayout.StarX(layout, star);
                _canvas.Icon(icon, x, top + HudLayout.BarHeight / 2 - _canvas.IconHeight(icon) / 2, 1, Color.white);
            }
        }

        private void DrawCombo(float time)
        {
            var multiplier = _score.ComboMultiplier;
            if (multiplier > _lastCombo) _comboRisenAt = time;
            _lastCombo = multiplier;
            if (multiplier < MinComboShown) return;
            _combo.Set(multiplier);
            var x = PixelGrid.Width - Margin - HudLayout.ComboWidth;
            var y = Margin - 1 + ComboWobble.OffsetY(time - _comboRisenAt);
            _canvas.Panel(x, y, HudLayout.ComboWidth, HudLayout.ComboHeight, PixelPalette.PanelCombo);
            var width = PixelTextLayout.Width(_combo.Text.Length, HudLayout.TextScale);
            _canvas.Text(_combo.Text, PixelTextLayout.Centered(x, HudLayout.ComboWidth, width), y + HudLayout.ComboTextY,
                PixelPalette.Cream, HudLayout.TextScale);
        }
    }
}
