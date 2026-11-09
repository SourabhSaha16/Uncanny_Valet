namespace UncannyValet.UI.Kit
{
    /// <summary>An integer rectangle on the virtual pixel canvas.</summary>
    public readonly struct PixelRect
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;

        public PixelRect(int x, int y, int width, int height)
        {
            X = x; Y = y; Width = width; Height = height;
        }

        public bool Contains(int px, int py) => px >= X && px < X + Width && py >= Y && py < Y + Height;
    }
}
