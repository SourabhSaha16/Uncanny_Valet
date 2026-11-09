using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Draws a licence plate with the pixel kit: a light plate, a border and the plate text in the pixel font.
    /// The text is a cached string held by the ticket row, so drawing allocates nothing.
    /// </summary>
    public sealed class PlatePainter
    {
        private const float GlowAlpha = 0.55f;

        private readonly PixelCanvas _canvas;

        public PlatePainter(PixelCanvas canvas)
        {
            _canvas = canvas;
        }

        /// <summary>Draws the plate with its top-left corner at (x, y).</summary>
        public void Draw(string plate, int x, int y, Color border)
        {
            var width = PlateLayout.Width(plate.Length);
            var height = PlateLayout.Height;
            _canvas.Fill(x, y, width, height, border);
            _canvas.Fill(x + PlateLayout.Border, y + PlateLayout.Border,
                width - 2 * PlateLayout.Border, height - 2 * PlateLayout.Border, PixelPalette.Cream);
            _canvas.Text(plate, x + PlateLayout.Border + PlateLayout.PadX, y + PlateLayout.Border + PlateLayout.PadY,
                PixelPalette.Ink, 1, false);
        }

        /// <summary>Draws the plate with a glow of <paramref name="glow"/> colour around it and a border of the same colour.</summary>
        public void DrawGlowing(string plate, int x, int y, Color glow)
        {
            var edge = PlateLayout.Glow;
            var faded = glow;
            faded.a = GlowAlpha;
            _canvas.Fill(x - edge, y - edge, PlateLayout.Width(plate.Length) + 2 * edge, PlateLayout.Height + 2 * edge, faded);
            Draw(plate, x, y, glow);
        }
    }
}
