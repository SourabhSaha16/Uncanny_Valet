namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// Lets the car that drives a trip tell the trip what is in front of it, so the trip can brake and wait.
    /// Implemented by the same object as <see cref="ITripDriver"/>; the car reports before every step.
    /// </summary>
    public interface ITripObstacles
    {
        /// <summary>
        /// Reports the bumper gap to the nearest solid thing ahead of trip <paramref name="tripId"/>
        /// (<see cref="float.PositiveInfinity"/> when the way is clear). Unknown ids are ignored.
        /// </summary>
        void SetObstacleGap(int tripId, float gap);
    }
}
