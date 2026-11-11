namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// The scale applied to world time while a power (freeze time) is active. Implemented by the Powers module.
    /// Modules that run their own time (the shift clock, Director, patron and wash timers, ambient traffic and
    /// pedestrians) multiply their delta time by <see cref="Scale"/>. The valet and the car the player drives ignore it.
    /// </summary>
    public interface IWorldTime
    {
        /// <summary>0 = fully frozen, 1 = normal speed. Never negative.</summary>
        float Scale { get; }

        /// <summary>True while <see cref="Scale"/> is below 1.</summary>
        bool IsSlowed { get; }
    }
}
