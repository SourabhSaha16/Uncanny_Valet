namespace UncannyValet.UI.Hud
{
    /// <summary>Where the three power bolts sit: a horizontal row at the right edge of the 480x270 canvas, bolt 1 leftmost.</summary>
    public static class BoltRowLayout
    {
        /// <summary>Number of bolts.</summary>
        public const int Count = 3;

        /// <summary>Canvas pixels per bolt pixel.</summary>
        public const int Scale = 2;

        /// <summary>Gap between two bolts.</summary>
        public const int Gap = 4;

        /// <summary>Top edge of the row.</summary>
        public const int Top = 46;

        /// <summary>Width of one bolt including its outline.</summary>
        public const int BoltWidth = (BoltGlyph.Width + 2) * Scale;

        /// <summary>Height of one bolt including its outline.</summary>
        public const int BoltHeight = (BoltGlyph.Height + 2) * Scale;

        /// <summary>Width of the whole row.</summary>
        public const int Width = Count * BoltWidth + (Count - 1) * Gap;

        /// <summary>Left edge of the row for a canvas of the given width and right margin.</summary>
        public static int Left(int canvasWidth, int margin) => canvasWidth - margin - Width;

        /// <summary>Left edge of bolt <paramref name="bolt"/> (1 to 3).</summary>
        public static int BoltX(int rowLeft, int bolt) => rowLeft + (bolt - 1) * (BoltWidth + Gap);
    }
}
