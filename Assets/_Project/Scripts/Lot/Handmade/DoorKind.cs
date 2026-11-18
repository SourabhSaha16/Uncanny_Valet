namespace UncannyValet.Lot.Handmade
{
    /// <summary>Which people use a door. Stored as an integer in prefabs: append only.</summary>
    public enum DoorKind
    {
        /// <summary>Patrons only.</summary>
        Main = 0,
        /// <summary>Ambient people only.</summary>
        Side = 1,
    }
}
