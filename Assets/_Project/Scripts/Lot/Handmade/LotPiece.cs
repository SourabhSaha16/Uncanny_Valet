namespace UncannyValet.Lot.Handmade
{
    /// <summary>A named rectangle with an index: the curb (0), a queue slot (its index, 0 nearest the curb), a solid, a hazard (size = width and height, height 0 for a round puddle) or a scenery prop.</summary>
    public readonly struct LotPiece
    {
        public readonly string Name;
        public readonly OrientedRect Rect;
        public readonly int Index;

        public LotPiece(string name, OrientedRect rect, int index = 0)
        {
            Name = name;
            Rect = rect;
            Index = index;
        }
    }
}
