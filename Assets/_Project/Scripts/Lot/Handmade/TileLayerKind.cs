namespace UncannyValet.Lot.Handmade
{
    /// <summary>The tilemap layers of a hand-made lot. Building and Hedge must be solid. Stored as an integer in prefabs: append only.</summary>
    public enum TileLayerKind
    {
        Ground = 0,
        Sidewalk = 1,
        Markings = 2,
        Building = 3,
        Hedge = 4,
        Overhead = 5,
    }
}
