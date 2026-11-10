namespace UncannyValet.Cars
{
    /// <summary>How a road trip stands after a step.</summary>
    public enum TripStatus
    {
        /// <summary>Still driving; the pose is the next one to apply.</summary>
        Running,
        /// <summary>The trip reached its end; the pose is the final one.</summary>
        Finished,
        /// <summary>The router no longer knows the trip; there is no pose.</summary>
        Lost,
    }
}
