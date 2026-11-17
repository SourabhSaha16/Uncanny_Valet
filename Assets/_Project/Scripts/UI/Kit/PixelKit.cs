using System;
using UnityEngine;

namespace UncannyValet.UI.Kit
{
    /// <summary>
    /// Owns every generated texture of the pixel UI (the nine-slice panel and the icons, made once in code with Point filtering and
    /// released with <see cref="Dispose"/>) and the TrueType font renderer.
    /// </summary>
    public sealed class PixelKit : IDisposable
    {
        /// <summary>Width and height of the panel texture; its border is <see cref="PanelBorder"/> pixels.</summary>
        public const int PanelSize = 9;
        public const int PanelBorder = 3;
        private const byte BevelShade = 0xC0;

        private readonly PixelFontRenderer _fontRenderer;
        private readonly Texture2D _panel;
        private readonly Texture2D[] _icons;

        public PixelKit()
        {
            _fontRenderer = new PixelFontRenderer(UiFontConfig.LoadOrDefault());
            _panel = BuildPanel();
            _icons = new Texture2D[Enum.GetValues(typeof(PixelIcon)).Length];
            for (var i = 0; i < _icons.Length; i++)
            {
                var icon = (PixelIcon)i;
                _icons[i] = PixelBitmap.FromRows("Pixel" + icon, PixelIconData.Rows(icon), PixelIconData.Keys, PixelIconData.Colors);
            }
        }

        /// <summary>Draws letters with the UI font.</summary>
        public PixelFontRenderer FontRenderer => _fontRenderer;

        /// <summary>The nine-slice panel: ink outline, a darker bevel, white fill (tint it to colour the panel).</summary>
        public Texture2D Panel => _panel;

        public Texture2D Icon(PixelIcon icon) => _icons[(int)icon];

        public void Dispose()
        {
            Release(_panel);
            for (var i = 0; i < _icons.Length; i++) Release(_icons[i]);
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
