using System.Text;
using UncannyValet.Core.Contracts;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// The corner HUD from the style guide's Scene 10: shift clock top centre, the blessing count with its bar and threshold top left,
    /// favour badge top right. Strings are cached and rebuilt only when a number changes.
    /// </summary>
    public sealed class HudPainter
    {
        private const int Margin = HudLayout.Margin;
        private const int ClockWidth = 58;
        private const int ClockHeight = 22;
        private const int BigScale = 2;
        private const int MinFavourShown = 2;
        private const float BlinkPeriod = 0.5f;
        private const float NeverRisen = -100f;

        private readonly PixelCanvas _canvas;
        private readonly IGameClock _clock;
        private readonly IShiftBlessingsView _blessingsView;
        private readonly ILotGoal _goal;
        private readonly StringBuilder _clockText = new StringBuilder(8);
        private readonly CachedText _blessings = new CachedText(string.Empty);
        private readonly StringBuilder _goalText = new StringBuilder(12);
        private readonly CachedText _favour = new CachedText("X");
        private int _clockSeconds = -1;
        private int _goalBlessings = -1;
        private int _lastBlessings;
        private float _gainedAt = NeverRisen;
        private int _lastFavour = 1;
        private float _favourRisenAt = NeverRisen;

        /// <param name="goal">The lot goal that sets the bar and the threshold; null draws the count only.</param>
        public HudPainter(PixelCanvas canvas, IGameClock clock, IShiftBlessingsView blessings, ILotGoal goal)
        {
            _canvas = canvas;
            _clock = clock;
            _blessingsView = blessings;
            _goal = goal;
        }

        public void Draw(float time)
        {
            DrawClock(time);
            DrawBlessings(time);
            DrawFavour(time);
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

        private void DrawBlessings(float time)
        {
            var total = _blessingsView.Blessings;
            if (total > _lastBlessings) _gainedAt = time;
            _lastBlessings = total;
            _blessings.Set(total);
            var x = Margin;
            var y = Margin - 1;
            _canvas.Panel(x, y, HudLayout.BlessingsWidth, HudLayout.BlessingsHeight, PixelPalette.PanelDark);
            _canvas.Icon(PixelIcon.Bolt, x + HudLayout.BoltX, y + HudLayout.BoltY, 1, Color.white);
            _canvas.Text(_blessings.Text, x + HudLayout.CountX, y + HudLayout.CountY, PixelPalette.Gold, HudLayout.TextScale);
            if (_goal != null) DrawBar(x, y, time);
        }

        private void DrawBar(int panelX, int panelY, float time)
        {
            var threshold = _goal.BlessingThreshold;
            var layout = new BlessingBarLayout(_lastBlessings, threshold, HudLayout.BarWidth);
            var left = panelX + HudLayout.BarX;
            var top = panelY + HudLayout.BarY;
            var fill = layout.Reached ? PixelPalette.Green : PixelPalette.Gold;
            fill = Color.Lerp(fill, PixelPalette.White, BlessingPulse.Strength(time - _gainedAt));
            _canvas.Fill(left - 1, top - 1, HudLayout.BarWidth + 2, HudLayout.BarHeight + 2, PixelPalette.Track);
            _canvas.Fill(left, top, layout.Fill, HudLayout.BarHeight, fill);
            _canvas.Fill(left + HudLayout.BarWidth - HudLayout.MarkerWidth, top - HudLayout.MarkerOverhang,
                HudLayout.MarkerWidth, HudLayout.BarHeight + HudLayout.MarkerOverhang * 2, PixelPalette.Cream);
            if (_goalBlessings != _lastBlessings)
            {
                _goalBlessings = _lastBlessings;
                _goalText.Clear().Append(_lastBlessings).Append(" / ").Append(threshold);
            }
            _canvas.Text(_goalText, panelX + HudLayout.BarX, panelY + HudLayout.GoalTextY, PixelPalette.Cream, HudLayout.TextScale);
        }

        private void DrawFavour(float time)
        {
            var multiplier = _blessingsView.FavourMultiplier;
            if (multiplier > _lastFavour) _favourRisenAt = time;
            _lastFavour = multiplier;
            if (multiplier < MinFavourShown) return;
            _favour.Set(multiplier);
            var x = PixelGrid.Width - Margin - HudLayout.FavourWidth;
            var y = Margin - 1 + FavourWobble.OffsetY(time - _favourRisenAt);
            _canvas.Panel(x, y, HudLayout.FavourWidth, HudLayout.FavourHeight, PixelPalette.PanelFavour);
            var width = PixelTextLayout.Width(_favour.Text.Length, HudLayout.TextScale);
            _canvas.Text(_favour.Text, PixelTextLayout.Centered(x, HudLayout.FavourWidth, width), y + HudLayout.FavourTextY,
                PixelPalette.Cream, HudLayout.TextScale);
        }
    }
}
