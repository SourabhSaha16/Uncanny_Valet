using System;
using UnityEngine;

namespace UncannyValet.UI.Kit
{
    /// <summary>
    /// Owns every generated texture of the pixel UI: the font atlas, the nine-slice panel and the icons. All are
    /// made once in code with Point filtering and released with <see cref="Dispose"/>.
    /// </summary>
    public sealed class PixelKit : IDisposable
    {
        /// <summary>Width and height of the panel texture; its border is <see cref="PanelBorder"/> pixels.</summary>
        public const int PanelSize = 9;
        public const int PanelBorder = 3;
        private const byte BevelShade = 0xC0;

        private readonly Texture2D _font;
        private readonly Texture2D _panel;
        private readonly Texture2D[] _icons;

        public PixelKit()
        {
            _font = BuildFont();
            _panel = BuildPanel();
            _icons = new Texture2D[Enum.GetValues(typeof(PixelIcon)).Length];
            for (var i = 0; i < _icons.Length; i++)
            {
                var icon = (PixelIcon)i;
                _icons[i] = PixelBitmap.FromRows("Pixel" + icon, PixelIconData.Rows(icon), PixelIconData.Keys, PixelIconData.Colors);
            }
        }

        /// <summary>The font atlas: one 5x7 cell per glyph, white on clear, in <see cref="PixelGlyphs.Chars"/> order.</summary>
        public Texture2D Font => _font;

        /// <summary>The nine-slice panel: ink outline, a darker bevel, white fill (tint it to colour the panel).</summary>
        public Texture2D Panel => _panel;

        public Texture2D Icon(PixelIcon icon) => _icons[(int)icon];

        public void Dispose()
        {
            Release(_font);
            Release(_panel);
            for (var i = 0; i < _icons.Length; i++) Release(_icons[i]);
        }

        private static Texture2D BuildFont()
        {
            var width = PixelGlyphs.Count * PixelGlyphs.Width;
            var height = PixelGlyphs.Height;
            var pixels = new Color32[width * height];
            var white = new Color32(255, 255, 255, 255);
            for (var glyph = 0; glyph < PixelGlyphs.Count; glyph++)
                for (var row = 0; row < height; row++)
                    for (var column = 0; column < PixelGlyphs.Width; column++)
                        if (PixelGlyphs.IsLit(glyph, row, column))
                            pixels[(height - 1 - row) * width + glyph * PixelGlyphs.Width + column] = white;
            return PixelBitmap.Make("PixelFont", width, height, pixels);
        }

        private static Texture2D BuildPanel()
        {
            var pixels = new Color32[PanelSize * PanelSize];
            var ink = (Color32)PixelPalette.Ink;
            var bevel = new Color32(BevelShade, BevelShade, BevelShade, 255);
            var fill = new Color32(255, 255, 255, 255);
            for (var y = 0; y < PanelSize; y++)
            {
                for (var x = 0; x < PanelSize; x++)
                {
                    var edge = Math.Min(Math.Min(x, y), Math.Min(PanelSize - 1 - x, PanelSize - 1 - y));
                    pixels[y * PanelSize + x] = edge == 0 ? ink : edge < PanelBorder ? bevel : fill;
                }
            }
            return PixelBitmap.Make("PixelPanel", PanelSize, PanelSize, pixels);
        }

        private static void Release(UnityEngine.Object texture)
        {
            if (texture == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(texture);
            else UnityEngine.Object.DestroyImmediate(texture);
        }
    }
}
