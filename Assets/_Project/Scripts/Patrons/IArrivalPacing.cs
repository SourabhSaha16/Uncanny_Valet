namespace UncannyValet.Patrons
{
    /// <summary>
    /// Arrival pacing chosen by the lot kind (issue 132). A queue-topping pacing makes the spawner fill every free queue
    /// slot on its own; a director pacing leaves arrivals to the Patron Director and the fixed timer.
    /// </summary>
    public interface IArrivalPacing
    {
        /// <summary>True when the spawner keeps the queue full and ignores the Patron Director's arrival gate.</summary>
        bool TopsUpQueue { get; }

        /// <summary>True when a queue-topping spawn may happen at this shift minute (the spacing since the last one has passed).</summary>
        bool Due(float elapsedMinutes);

        /// <summary>Records that a car was sent at this shift minute.</summary>
        void Spawned(float elapsedMinutes);

        /// <summary>Forgets earlier spawns; called at every shift start.</summary>
        void Reset();
    }
}
