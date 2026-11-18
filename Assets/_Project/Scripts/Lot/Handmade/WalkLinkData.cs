namespace UncannyValet.Lot.Handmade
{
    /// <summary>A walkable link between two walk nodes (indices into <see cref="HandmadeLotData.WalkNodes"/>).</summary>
    public readonly struct WalkLinkData
    {
        public readonly string Name;
        public readonly int A;
        public readonly int B;
        public readonly bool Crosswalk;

        public WalkLinkData(string name, int a, int b, bool crosswalk = false)
        {
            Name = name;
            A = a;
            B = b;
            Crosswalk = crosswalk;
        }
    }
}
