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

        /// <summary>How often an arriving car re-checks which queue slot it should take, in seconds.</summary>
        public readonly float RetargetSeconds;

        /// <summary>Default for <see cref="RetargetSeconds"/>.</summary>
        public const float DefaultRetargetSeconds = 0.2f;

        public TripTuning(float carLength, float turnSpeed, float routeSpeed, float yieldDistance, float arriveTolerance,
            float retargetSeconds = DefaultRetargetSeconds)
        {
            RetargetSeconds = retargetSeconds;
            CarLength = carLength;
            TurnSpeed = turnSpeed;
            RouteSpeed = routeSpeed;
            YieldDistance = yieldDistance;
            ArriveTolerance = arriveTolerance;
        }
    }
}
