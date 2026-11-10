namespace UncannyValet.Traffic
{
    /// <summary>Steps trips. The car's module calls this every physics step and moves the car to the returned pose.</summary>
    public interface ITripDriver
    {
        /// <summary>
        /// Advances a trip. Returns false if the id is unknown or already finished. The step whose pose has
        /// <see cref="TripPhase.Finished"/> is the last one; the id is released after it.
        /// </summary>
        bool Step(int tripId, float dt, out TripPose pose);

        /// <summary>Ends a trip early (the car was destroyed) and frees everything it holds.</summary>
        void Cancel(int tripId);
    }
}
