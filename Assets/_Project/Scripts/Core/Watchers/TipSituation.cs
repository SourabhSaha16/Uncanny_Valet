namespace UncannyValet.Core.Watchers
{
    /// <summary>
    /// The situations that make a watcher say a tip. Stored as an integer in the watcher text assets: append-only, never remove,
    /// reorder or insert in the middle.
    /// </summary>
    public enum TipSituation
    {
        /// <summary>The first patron of the shift handed over keys.</summary>
        FirstArrival = 0,
        /// <summary>A Muddy car arrived.</summary>
        MuddyCar = 1,
        /// <summary>A car that needs a charge arrived.</summary>
        ChargeCar = 2,
        /// <summary>A patron's patience is nearly gone.</summary>
        PatienceLow = 3,
        /// <summary>Every arrival queue slot holds a waiting car.</summary>
        QueueFull = 4,
        /// <summary>A car took damage.</summary>
        CarDamaged = 5
    }
}
