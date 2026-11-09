using System;

namespace UncannyValet.UI.Kit
{
    /// <summary>
    /// Maps the virtual 480x270 pixel canvas onto the screen: the largest whole-number scale that fits, centred,
    /// so every HUD pixel is a crisp square at any window size. Pure.
    /// </summary>
    public readonly struct PixelGrid
    {
        public const int Width = 480;
        public const int Height = 270;

        public readonly int Scale;
        public readonly int OffsetX;
        public readonly int OffsetY;

        public PixelGrid(int screenWidth, int screenHeight)
        {
            Scale = Math.Max(1, Math.Min(screenWidth / Width, screenHeight / Height));
            OffsetX = (screenWidth - Width * Scale) / 2;
            OffsetY = (screenHeight - Height * Scale) / 2;
        }

        /// <summary>Screen pixel of a virtual x.</summary>
        public int ScreenX(int x) => OffsetX + x * Scale;

        /// <summary>Screen pixel of a virtual y (top-down, like IMGUI).</summary>
        public int ScreenY(int y) => OffsetY + y * Scale;

        /// <summary>Virtual column under a screen x (IMGUI coordinates).</summary>
        public int VirtualX(float screenX) => (int)Math.Floor((screenX - OffsetX) / Scale);

        /// <summary>Virtual row under a screen y (IMGUI coordinates, top-down).</summary>
        public int VirtualY(float screenY) => (int)Math.Floor((screenY - OffsetY) / Scale);
    }
}
