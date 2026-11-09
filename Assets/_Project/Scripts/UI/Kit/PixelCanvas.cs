using System.Text;
using UnityEngine;

namespace UncannyValet.UI.Kit
{
    /// <summary>
    /// Draws on the virtual pixel canvas with IMGUI. Every size and position is in virtual pixels; they become
    /// whole screen pixels through <see cref="PixelGrid"/>. Call from a Repaint event only.
    /// </summary>
    public sealed class PixelCanvas
    {
        private readonly PixelKit _kit;
        private PixelGrid _grid;

        public PixelCanvas(PixelKit kit)
        {
            _kit = kit;
        }

        public PixelGrid Grid => _grid;

        /// <summary>Screen height in pixels, for turning bottom-up screen points into canvas rows.</summary>
        public int ScreenHeight { get; private set; }

        public void Begin(int screenWidth, int screenHeight)
        {
            _grid = new PixelGrid(screenWidth, screenHeight);
            ScreenHeight = screenHeight;
        }

        public void End() => GUI.color = Color.white;

        public int IconWidth(PixelIcon icon) => _kit.Icon(icon).width;

        public int IconHeight(PixelIcon icon) => _kit.Icon(icon).height;

        public void Fill(int x, int y, int width, int height, Color color)
        {
            if (width <= 0 || height <= 0) return;
            GUI.color = color;
            GUI.DrawTexture(ScreenRect(x, y, width, height), Texture2D.whiteTexture);
        }

        /// <summary>A thick-bordered panel; <paramref name="fill"/> tints the inside and bevel.</summary>
        public void Panel(int x, int y, int width, int height, Color fill)
        {
            const int b = PixelKit.PanelBorder;
            if (width < b * 2 || height < b * 2) return;
            GUI.color = fill;
            for (var row = 0; row < 3; row++)
            {
                var dy = row == 0 ? y : row == 1 ? y + b : y + height - b;
                var dh = row == 1 ? height - b * 2 : b;
                for (var column = 0; column < 3; column++)
                {
                    var dx = column == 0 ? x : column == 1 ? x + b : x + width - b;
                    var dw = column == 1 ? width - b * 2 : b;
                    Blit(_kit.Panel, dx, dy, dw, dh, column * b, row * b, b, b);
                }
            }
        }

        public void Icon(PixelIcon icon, int x, int y, int scale, Color tint)
        {
            var texture = _kit.Icon(icon);
            GUI.color = tint;
            GUI.DrawTexture(ScreenRect(x, y, texture.width * scale, texture.height * scale), texture);
        }

        /// <summary>One glyph; spaces draw nothing.</summary>
        public void Char(char c, int x, int y, Color color, int scale)
        {
            if (c == ' ') return;
            GUI.color = color;
            Blit(_kit.Font, x, y, PixelGlyphs.Width * scale, PixelGlyphs.Height * scale,
                PixelGlyphs.IndexOf(c) * PixelGlyphs.Width, 0, PixelGlyphs.Width, PixelGlyphs.Height);
        }

        public void Text(string text, int x, int y, Color color, int scale = 1, bool shadow = true)
        {
            if (shadow) Run(text, x + ShadowOffset(scale), y + ShadowOffset(scale), PixelPalette.Ink, scale);
            Run(text, x, y, color, scale);
        }

        public void Text(StringBuilder text, int x, int y, Color color, int scale = 1, bool shadow = true)
        {
            if (shadow) Lines(text, x + ShadowOffset(scale), y + ShadowOffset(scale), PixelPalette.Ink, scale);
            Lines(text, x, y, color, scale);
        }

        /// <summary>Pixel offset of the drop shadow; chunkier for big letters.</summary>
        public static int ShadowOffset(int scale) => scale >= 4 ? 2 : 1;

        private void Run(string text, int x, int y, Color color, int scale)
        {
            for (var i = 0; i < text.Length; i++)
                Char(text[i], x + i * PixelTextLayout.Advance * scale, y, color, scale);
        }

        // Newline starts a new line under the first one.
        private void Lines(StringBuilder text, int x, int y, Color color, int scale)
        {
            var column = 0;
            var top = y;
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    column = 0;
                    top += PixelTextLayout.LineHeight(scale);
                    continue;
                }
                Char(text[i], x + column * PixelTextLayout.Advance * scale, top, color, scale);
                column++;
            }
        }

        private Rect ScreenRect(int x, int y, int width, int height)
            => new Rect(_grid.ScreenX(x), _grid.ScreenY(y), width * _grid.Scale, height * _grid.Scale);

        private void Blit(Texture2D texture, int x, int y, int width, int height, int srcX, int srcY, int srcW, int srcH)
        {
            var uv = new Rect(srcX / (float)texture.width, srcY / (float)texture.height,
                srcW / (float)texture.width, srcH / (float)texture.height);
            GUI.DrawTextureWithTexCoords(ScreenRect(x, y, width, height), texture, uv);
        }
    }
}
