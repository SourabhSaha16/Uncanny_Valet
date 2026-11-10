namespace UncannyValet.Traffic
{
    /// <summary>The distances and speeds a patron trip needs besides car following and merge rules.</summary>
    public readonly struct TripTuning
    {
        /// <summary>Bumper to bumper car length used for gaps.</summary>
        public readonly float CarLength;
        /// <summary>Speed before and during the turn into the queue.</summary>
        public readonly float TurnSpeed;
        /// <summary>Speed along the curb exit route.</summary>
        public readonly float RouteSpeed;
        /// <summary>Distance before the merge point at which a leaving car starts yielding.</summary>
        public readonly float YieldDistance;
        /// <summary>A route counts as finished this close to its end.</summary>
        public readonly float ArriveTolerance;

        public TripTuning(float carLength, float turnSpeed, float routeSpeed, float yieldDistance, float arriveTolerance)
        {
            CarLength = carLength;
            TurnSpeed = turnSpeed;
            RouteSpeed = routeSpeed;
            YieldDistance = yieldDistance;
            ArriveTolerance = arriveTolerance;
        }
    }
}
