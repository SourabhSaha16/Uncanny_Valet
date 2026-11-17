namespace UncannyValet.Core.Ids
{
    /// <summary>The nine worlds of the run, in story order. Stored as integers in assets: append only, never reorder.</summary>
    public enum World
    {
        /// <summary>Unset; not a real world.</summary>
        None = 0,
        Midgard = 1,
        Vanaheim = 2,
        Niflheim = 3,
        Svartalfheim = 4,
        Alfheim = 5,
        Muspelheim = 6,
        Jotunheim = 7,
        Helheim = 8,
        /// <summary>The finale; its only lot is <see cref="LotKind.Finale"/>.</summary>
        Asgard = 9,
    }
}
