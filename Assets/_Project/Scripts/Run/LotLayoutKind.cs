namespace UncannyValet.Run
{
    /// <summary>
    /// Which parametric layout a lot is built from. Later world tasks append kinds (ring, pillars, cliff, ...).
    /// Stored as an integer in assets: append only, never reorder.
    /// </summary>
    public enum LotLayoutKind
    {
        /// <summary>Unset; a lot must pick a layout.</summary>
        None = 0,
        /// <summary>Today's diner lot: two rows of four bays, a curb, a 3-slot queue, a wash slot and an EV bay.</summary>
        Diner = 1,
        /// <summary>A lot an artist authored by hand as a prefab (tilemaps, scenery and gameplay markers); see <see cref="LotDefinition.HandmadePrefab"/>.</summary>
        Handmade = 2,
    }
}
