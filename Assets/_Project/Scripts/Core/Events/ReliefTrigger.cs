namespace UncannyValet.Core.Events
{
    /// <summary>What pushed the player into the drowning state that starts relief.</summary>
    public enum ReliefTrigger
    {
        /// <summary>Workload above the relief threshold.</summary>
        Workload,

        /// <summary>A patron is close to anger.</summary>
        Pressure,

        /// <summary>Several patrons stormed off recently.</summary>
        Angry
    }
}
