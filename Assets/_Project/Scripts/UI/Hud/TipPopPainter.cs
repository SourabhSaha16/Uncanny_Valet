using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Draws the live tip pops in the 480x270 pixel canvas: the amount rises a few pixels from where the patron boarded and
    /// fades out. Uses the same world-to-canvas projection and clamping as the mood bubble and the world tickets.
    /// </summary>
    public sealed class TipPopPainter
    {
        private readonly PixelCanvas _canvas;
        private readonly TipPops _pops;
        private readonly TipPopSettings _settings;

        public TipPopPainter(PixelCanvas canvas, TipPops pops, TipPopSettings settings)
        {
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

        private void DrawPop(Camera camera, TipPops.Pop pop, float age)
        {
            var screen = camera.WorldToScreenPoint(new Vector3(pop.World.x, pop.World.y, 0f));
            if (screen.z < 0f) return;
            var scale = pop.Kind == TipPopKind.Big ? _settings.BigFontScale : _settings.FontScale;
            var width = PixelTextLayout.Width(pop.Text.Length, scale);
            var height = PixelTextLayout.Height(scale);
            var rise = Mathf.RoundToInt(_settings.RisePixels * TipPopText.Rise(age, _settings.DurationSeconds));
            var x = WorldTicketLayout.ClampToCanvas(_canvas.Grid.VirtualX(screen.x) - width / 2, width, PixelGrid.Width);
            var y = WorldTicketLayout.ClampToCanvas(
                _canvas.Grid.VirtualY(_canvas.ScreenHeight - screen.y) - rise - height, height, PixelGrid.Height);
            var alpha = TipPopText.Alpha(age, _settings.DurationSeconds, _settings.HoldFraction);
            var shadow = PixelCanvas.ShadowOffset(scale);
            _canvas.Text(pop.Text, x + shadow, y + shadow, WithAlpha(PixelPalette.Ink, alpha), scale, false);
            _canvas.Text(pop.Text, x, y, WithAlpha(ColorOf(pop.Kind), alpha), scale, false);
        }

        private static Color ColorOf(TipPopKind kind)
        {
            switch (kind)
            {
                case TipPopKind.Big: return PixelPalette.Gold;
                case TipPopKind.Normal: return PixelPalette.Green;
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
