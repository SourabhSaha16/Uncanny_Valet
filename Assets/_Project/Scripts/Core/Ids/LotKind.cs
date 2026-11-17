namespace UncannyValet.Core.Ids
{
    /// <summary>What sort of lot a world offers. Stored as integers in assets: append only, never reorder.</summary>
    public enum LotKind
    {
        /// <summary>Unset; not a real kind.</summary>
        None = 0,
        Training = 1,
        Crazy = 2,
        /// <summary>The single Asgard level.</summary>
        Finale = 3,
    }
}
