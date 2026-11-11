using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Draws the Park Smash look on the 480x270 canvas. <see cref="DrawWorld"/> paints the shockwave ring and the particles
    /// under the HUD; <see cref="DrawOverlay"/> paints the white flash and the SMASH! pop over everything. Draws nothing
    /// while the look is off and allocates nothing.
    /// </summary>
    public sealed class SmashFxPainter
    {
        private readonly PixelCanvas _canvas;
        private readonly SmashFx _fx;

        public SmashFxPainter(PixelCanvas canvas, SmashFx fx)
        {
            _canvas = canvas;
            _fx = fx;
        }

        /// <summary>Ring and particles; <paramref name="now"/> is unscaled seconds.</summary>
        public void DrawWorld(Camera camera, float now)
        {
            if (camera == null) return;
            _fx.Expire(now);
            DrawRing(camera, now);
            for (var i = 0; i < _fx.Capacity; i++)
            {
                var p = _fx[i];
                if (p.Active) DrawParticle(camera, p, now - p.StartedAt);
            }
        }

        /// <summary>Flash and pop.</summary>
        public void DrawOverlay(float now)
        {
            var flash = SmashLook.FlashAlpha(_fx.FlashAge(now));
            if (flash > 0f) _canvas.Fill(0, 0, PixelGrid.Width, PixelGrid.Height, WithAlpha(SmashLook.FlashColor, flash));
            var age = _fx.PopAge(now);
            var alpha = SmashLook.PopAlpha(age);
            if (alpha <= 0f) return;
            var text = _fx.PopText;
            var scale = SmashLook.PopScale;
            var x = PixelTextLayout.Centered(0, PixelGrid.Width, PixelTextLayout.Width(text.Length, scale));
            var y = SmashLook.PopTop - SmashLook.PopRise(age);
            var shadow = PixelCanvas.ShadowOffset(scale);
            _canvas.Text(text, x + shadow, y + shadow, WithAlpha(PixelPalette.Ink, alpha), scale, false);
            _canvas.Text(text, x, y, WithAlpha(SmashLook.PopColor, alpha), scale, false);
        }

        private void DrawRing(Camera camera, float now)
        {
            var age = _fx.RingAge(now);
            var outer = SmashLook.RingRadius(age);
            if (outer < 0) return;
            var centre = RingCentre(camera);
            var inner = outer - SmashLook.RingThickness;
            var color = WithAlpha(SmashLook.RingColor, SmashLook.RingAlpha(age));
            var step = SmashLook.RingRowHeight;
            for (var y = System.Math.Max(0, centre.y - outer); y < System.Math.Min(PixelGrid.Height, centre.y + outer); y += step)
            {
                var offset = y + step / 2 - centre.y;
                var wide = SmashLook.HalfWidth(outer, offset);
                var hole = SmashLook.HalfWidth(inner, offset);
                if (hole == 0) { Span(centre.x - wide, centre.x + wide, y, step, color); continue; }
                Span(centre.x - wide, centre.x - hole, y, step, color);
                Span(centre.x + hole, centre.x + wide, y, step, color);
            }
        }

        private Vector2Int RingCentre(Camera camera)
        {
            if (!_fx.RingHasOrigin) return new Vector2Int(PixelGrid.Width / 2, PixelGrid.Height / 2);
            return ToCanvas(camera, _fx.RingOrigin, out _);
        }

        private void DrawParticle(Camera camera, SmashFx.Particle p, float age)
        {
            var at = ToCanvas(camera, SmashFx.PositionAt(p, age), out var visible);
            if (!visible) return;
            switch (p.Kind)
            {
                case SmashParticleKind.Sparkle: DrawSparkle(at, 1f - age / p.Life); break;
                case SmashParticleKind.Coin: _canvas.Fill(at.x, at.y, SmashLook.PieceSize, SmashLook.PieceSize, PixelPalette.Gold); break;
                default: _canvas.Fill(at.x, at.y, SmashLook.PieceSize, SmashLook.PieceSize, SmashLook.ConfettiColors[p.Shade]); break;
            }
        }

        private void DrawSparkle(Vector2Int at, float fade)
        {
            var arm = SmashLook.SparkleArm;
            var color = WithAlpha(PixelPalette.White, Mathf.Clamp01(fade));
            _canvas.Fill(at.x - arm, at.y, arm * 2 + 1, 1, color);
            _canvas.Fill(at.x, at.y - arm, 1, arm * 2 + 1, color);
        }

        private Vector2Int ToCanvas(Camera camera, Vector2 world, out bool visible)
        {
            var screen = camera.WorldToScreenPoint(new Vector3(world.x, world.y, 0f));
            var x = _canvas.Grid.VirtualX(screen.x);
            var y = _canvas.Grid.VirtualY(_canvas.ScreenHeight - screen.y);
            visible = screen.z >= 0f && x >= 0 && x < PixelGrid.Width && y >= 0 && y < PixelGrid.Height;
            return new Vector2Int(x, y);
        }

        private void Span(int from, int to, int y, int height, Color color)
        {
            from = System.Math.Max(0, from);
            to = System.Math.Min(PixelGrid.Width, to);
            if (to > from) _canvas.Fill(from, y, to - from, height, color);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
