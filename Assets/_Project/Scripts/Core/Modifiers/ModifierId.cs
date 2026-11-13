namespace UncannyValet.Core.Modifiers
{
    /// <summary>
    /// The service modifiers a car can carry (what it needs before it is handed back). Append-only: the numbers are
    /// stored in events and sets, so never remove, reorder or insert in the middle. At most
    /// <see cref="ModifierSet.Capacity"/> ids (None included) fit a <see cref="ModifierSet"/>.
    /// </summary>
    public enum ModifierId
    {
        /// <summary>No modifier; never held by a car.</summary>
        None = 0,

        /// <summary>Muddy: cleared at the Wash station.</summary>
        Muddy = 1,

        /// <summary>Needs a charge (the EV need): cleared at the Charging station.</summary>
        NeedsCharge = 2
    }
}
