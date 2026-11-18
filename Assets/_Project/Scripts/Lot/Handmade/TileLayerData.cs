namespace UncannyValet.Lot.Handmade
{
    /// <summary>One tilemap layer of the lot and whether it carries a collider.</summary>
    public readonly struct TileLayerData
    {
        public readonly string Name;
        public readonly TileLayerKind Kind;
        public readonly bool HasCollider;

        public TileLayerData(string name, TileLayerKind kind, bool hasCollider)
        {
            Name = name;
            Kind = kind;
            HasCollider = hasCollider;
        }

        /// <summary>True for the layers that must stop cars and the valet.</summary>
        public bool MustBeSolid => Kind == TileLayerKind.Building || Kind == TileLayerKind.Hedge;
    }
}
