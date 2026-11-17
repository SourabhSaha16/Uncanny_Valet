using System;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Draws the live blessing pops in the 480x270 pixel canvas: the amount rises a few pixels from where the patron boarded and
    /// fades out. Uses the same world-to-canvas projection and clamping as the mood bubble and the world tickets.
    /// </summary>
    public sealed class BlessingPopPainter
    {
        private const int IconGap = 2;

        private readonly PixelCanvas _canvas;
        private readonly BlessingPops _pops;
        private readonly BlessingPopSettings _settings;
        private readonly Color _perfectColor;

        public BlessingPopPainter(PixelCanvas canvas, BlessingPops pops, BlessingPopSettings settings, Color perfectColor)
        {
            _perfectColor = perfectColor;
            _canvas = canvas;
            _pops = pops;
            _settings = settings;
        }

        public void Draw(Camera camera, float now)
        {
            if (camera == null) return;
            _pops.Expire(now);
            for (var i = 0; i < _pops.Capacity; i++)
            {
                var pop = _pops[i];
                if (pop.Active) DrawPop(camera, pop, now - pop.StartedAt);
            }
        }

        private void DrawPop(Camera camera, BlessingPops.Pop pop, float age)
        {
            var screen = camera.WorldToScreenPoint(new Vector3(pop.World.x, pop.World.y, 0f));
            if (screen.z < 0f) return;
            var scale = ScaleOf(pop.Kind);
            var textWidth = PixelTextLayout.Width(pop.Text.Length, scale);
            var width = pop.HasIcon ? textWidth + IconGap + _canvas.IconWidth(PixelIcon.PrayerHands) : textWidth;
            var height = pop.HasIcon ? Math.Max(PixelTextLayout.Height(scale), _canvas.IconHeight(PixelIcon.PrayerHands)) : PixelTextLayout.Height(scale);
            var rise = Mathf.RoundToInt(_settings.RisePixels * BlessingPopText.Rise(age, _settings.DurationSeconds));
            var x = WorldTicketLayout.ClampToCanvas(_canvas.Grid.VirtualX(screen.x) - width / 2, width, PixelGrid.Width);
            var y = WorldTicketLayout.ClampToCanvas(
                _canvas.Grid.VirtualY(_canvas.ScreenHeight - screen.y) - rise - height, height, PixelGrid.Height);
            var alpha = BlessingPopText.Alpha(age, _settings.DurationSeconds, _settings.HoldFraction);
            var shadow = PixelCanvas.ShadowOffset(scale);
            _canvas.Text(pop.Text, x + shadow, y + shadow, WithAlpha(PixelPalette.Ink, alpha), scale, false);
            _canvas.Text(pop.Text, x, y, WithAlpha(ColorOf(pop.Kind, _perfectColor), alpha), scale, false);
            if (pop.HasIcon) DrawIcon(x + textWidth + IconGap, y, alpha);
        }

        private void DrawIcon(int x, int y, float alpha)
        {
            _canvas.Icon(PixelIcon.PrayerHands, x, y, 1, WithAlpha(Color.white, alpha));
        }

        private int ScaleOf(BlessingPopKind kind)
        {
            switch (kind)
            {
                case BlessingPopKind.Big:
                case BlessingPopKind.Perfect: return _settings.BigFontScale;
                case BlessingPopKind.Curse: return _settings.CurseFontScale;
                default: return _settings.FontScale;
            }
        }

        private static Color ColorOf(BlessingPopKind kind, Color perfect)
        {
            switch (kind)
            {
                case BlessingPopKind.Curse: return PixelPalette.Blood;
                case BlessingPopKind.Big: return PixelPalette.Gold;
                case BlessingPopKind.Perfect: return perfect;
                case BlessingPopKind.Normal: return PixelPalette.Green;
                default: return PixelPalette.Grey;
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
