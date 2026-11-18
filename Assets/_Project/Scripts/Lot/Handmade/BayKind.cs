namespace UncannyValet.Lot.Handmade
{
    /// <summary>What a hand-placed bay is for. Stored as an integer in prefabs: append only, never reorder.</summary>
    public enum BayKind
    {
        Regular = 0,
        Charging = 1,
        Wash = 2,
    }
}
