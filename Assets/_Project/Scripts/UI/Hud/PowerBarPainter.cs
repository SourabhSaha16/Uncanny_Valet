using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Draws the power meter at the right edge of the 480x270 canvas: three lightning bolts side by side that fill one after
    /// another (each shows its own share of the bar), a full bolt pulsing and the bolt just spent flashing white, and under them
    /// one icon per earned power: its key digit and letter, dim when not usable, a square ring counting the running power or its
    /// cooldown, and a flash once at the first lot start after it was earned. Nothing is drawn before Thor owns a power.
    /// </summary>
    public sealed class PowerBarPainter
    {
        public const int ColumnWidth = BoltRowLayout.Width;
        public const int IconSide = 17;
        private const int KeyInset = 2;
        private const int LetterInset = 2;
        private const int Slots = 4;
        private const float FlashFill = 0.6f;
        public const int IconGap = 3;
        private const float PulseWhite = 0.55f;

        private readonly PixelCanvas _canvas;
        private readonly PowerBarTracker _tracker;

        public PowerBarPainter(PixelCanvas canvas, PowerBarTracker tracker)
        {
            _canvas = canvas;
            _tracker = tracker;
        }

        /// <summary>Draws the bolts and the power icons; time is unscaled seconds.</summary>
        public void Draw(float time)
        {
            _tracker.Tick(time);
            if (!_tracker.IsVisible) return;
            var left = BoltRowLayout.Left(PixelGrid.Width, HudLayout.Margin);
            for (var bolt = 1; bolt <= BoltRowLayout.Count; bolt++) DrawBolt(BoltRowLayout.BoltX(left, bolt), bolt, time);
            DrawPowerIcons();
        }

        private void DrawBolt(int x, int bolt, float time)
        {
            var view = _tracker.View;
            var rows = BoltFillMaths.LitRows(BoltFillMaths.Share(view.Fill, bolt), BoltGlyph.Height);
            var lit = PowerColors.Gold;
            if (view.OpenBuckets >= bolt) lit = Color.Lerp(lit, Color.white, BoltFillMaths.Pulse(time, _tracker.FullPulseHz) * PulseWhite);
            var empty = PowerColors.Dimmed(PowerColors.Gold);
            if (view.SpentBucket == bolt && view.Flash01 > 0f)
            {
                lit = Color.Lerp(lit, Color.white, view.Flash01);
                empty = Color.Lerp(empty, Color.white, view.Flash01);
            }
            BoltGlyph.DrawFilled(_canvas, x, BoltRowLayout.Top, BoltRowLayout.Scale, rows, lit, empty);
        }

        private void DrawPowerIcons()
        {
            var x = PixelGrid.Width - HudLayout.Margin - IconSide;
            var top = BoltRowLayout.Top + BoltRowLayout.BoltHeight + IconGap;
            for (var slot = 0; slot < _tracker.IconCount && slot < Slots; slot++)
            {
                var power = _tracker.PowerAt(slot);
                if (!_tracker.TryGetLook(power, out var look)) continue;
                DrawIcon(x, top + slot * (IconSide + IconGap), slot, power, look);
            }
        }

        private void DrawIcon(int x, int y, int slot, StoryPower power, PowerIconLook look)
        {
            var color = PowerColors.Of(power);
            var shown = look.Dimmed ? PowerColors.Dimmed(color) : color;
            var flash = _tracker.Flashes.Flash01(power);
            if (flash > 0f) shown = Color.Lerp(shown, Color.white, flash);
            _canvas.Fill(x, y, IconSide, IconSide, flash > 0f ? Color.Lerp(PixelPalette.Track, Color.white, flash * FlashFill) : PixelPalette.Track);
            DrawRing(x, y, look, color);
            _canvas.Char((char)('1' + slot), x + KeyInset, y + KeyInset, shown, 1);
            _canvas.Char(PowerColors.LetterOf(power), x + IconSide - PixelGlyphs.Width - LetterInset, y + IconSide - PixelGlyphs.Height - LetterInset, shown, 1);
        }

        private void DrawRing(int x, int y, PowerIconLook look, Color color)
        {
            var lit = SquareRing.Lit(IconSide, look.RingFraction);
            var ring = look.RingIsActive ? color : PowerColors.CooldownGrey;
            for (var i = 0; i < lit; i++)
            {
                SquareRing.Cell(IconSide, i, out var dx, out var dy);
                _canvas.Fill(x + dx, y + dy, 1, 1, ring);
            }
        }
    }
}
