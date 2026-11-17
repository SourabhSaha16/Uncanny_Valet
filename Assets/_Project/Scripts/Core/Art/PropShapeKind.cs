namespace UncannyValet.Core.Art
{
    /// <summary>What a <see cref="PropShape"/> draws inside its pixel rectangle. Stored as integers in assets: append-only.</summary>
    public enum PropShapeKind
    {
        /// <summary>A filled rectangle.</summary>
        Rect = 0,
        /// <summary>A filled ellipse.</summary>
        Ellipse = 1,
        /// <summary>An ellipse outline one pixel wide.</summary>
        Ring = 2,
        /// <summary>A filled triangle with its point at the top middle.</summary>
        TriangleUp = 3,
        /// <summary>A one pixel line from the bottom-left to the top-right corner.</summary>
        LineUp = 4,
        /// <summary>A one pixel line from the top-left to the bottom-right corner.</summary>
        LineDown = 5,
        /// <summary>The shape's text in the pixel font, drawn from the bottom-left of the rectangle.</summary>
        Text = 6,
    }
}
