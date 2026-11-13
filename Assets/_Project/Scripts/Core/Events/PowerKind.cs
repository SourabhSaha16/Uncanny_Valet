namespace UncannyValet.Core.Events
{
    /// <summary>The valet powers that have an effect. The speed boost (1) and Park Smash (3) were removed; their numbers stay unused.</summary>
    public enum PowerKind
    {
        /// <summary>The world stops for a short time; the valet and the driven car keep moving.</summary>
        FreezeTime = 2,
    }
}
