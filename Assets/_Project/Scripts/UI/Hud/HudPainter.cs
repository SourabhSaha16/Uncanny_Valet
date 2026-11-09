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
        private const int Margin = 4;
        private const int ClockWidth = 58;
        private const int ClockHeight = 22;
        private const int ScoreWidth = 112;
        private const int ScoreHeight = 40;
        private const int BarX = 8;
        private const int BarY = 27;
        private const int BarWidth = 96;
        private const int BarHeight = 5;
        private const int BigScale = 2;
        private const int StarOverhang = 4;
        private const int ComboWidth = 40;
        private const int ComboHeight = 24;
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
            _canvas.Panel(Margin, Margin - 1, ScoreWidth, ScoreHeight, PixelPalette.PanelDark);
            _canvas.Icon(PixelIcon.Coin, Margin + 6, Margin + 4, BigScale, Color.white);
            _canvas.Text(_tips.Text, Margin + 28, Margin + 5, PixelPalette.Gold, BigScale);
            DrawStarBar();
        }

        private void DrawStarBar()
        {
            var left = Margin + BarX;
            var top = Margin + BarY;
            var layout = new StarBarLayout(_score.Score, _score.OneStarScore, _score.TwoStarScore,
                _score.ThreeStarScore, BarWidth);
            _canvas.Fill(left - 1, top - 1, BarWidth + 2, BarHeight + 2, PixelPalette.Track);
            _canvas.Fill(left, top, layout.Fill, BarHeight, PixelPalette.Gold);
            for (var star = 0; star < StarBarLayout.Stars; star++)
            {
                var icon = layout.IsLit(star) ? PixelIcon.StarLit : PixelIcon.StarUnlit;
                var x = left + layout.MarkerX(star) - StarOverhang;
                _canvas.Icon(icon, x, top + BarHeight / 2 - _canvas.IconHeight(icon) / 2, 1, Color.white);
            }
        }

        private void DrawCombo(float time)
        {
            var multiplier = _score.ComboMultiplier;
            if (multiplier > _lastCombo) _comboRisenAt = time;
            _lastCombo = multiplier;
            if (multiplier < MinComboShown) return;
            _combo.Set(multiplier);
            var x = PixelGrid.Width - Margin - ComboWidth;
            var y = Margin - 1 + ComboWobble.OffsetY(time - _comboRisenAt);
            _canvas.Panel(x, y, ComboWidth, ComboHeight, PixelPalette.PanelCombo);
            var width = PixelTextLayout.Width(_combo.Text.Length, BigScale);
            _canvas.Text(_combo.Text, PixelTextLayout.Centered(x, ComboWidth, width), y + 5, PixelPalette.Cream, BigScale);
        }
    }
}
