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

        /// <summary>Seconds a trip car waits behind a solid obstacle before it creeps on.</summary>
        public readonly float BlockedTimeout;
        /// <summary>Speed of that creep, in units per second.</summary>
        public readonly float CreepSpeed;

        /// <summary>Default for <see cref="BlockedTimeout"/>.</summary>
        public const float DefaultBlockedTimeout = 6f;
        /// <summary>Default for <see cref="CreepSpeed"/>.</summary>
        public const float DefaultCreepSpeed = 0.8f;

        public TripTuning(float carLength, float turnSpeed, float routeSpeed, float yieldDistance, float arriveTolerance,
            float retargetSeconds = DefaultRetargetSeconds, float blockedTimeout = DefaultBlockedTimeout, float creepSpeed = DefaultCreepSpeed)
        {
            BlockedTimeout = blockedTimeout;
            CreepSpeed = creepSpeed;
            RetargetSeconds = retargetSeconds;
            CarLength = carLength;
            TurnSpeed = turnSpeed;
            RouteSpeed = routeSpeed;
            YieldDistance = yieldDistance;
            ArriveTolerance = arriveTolerance;
        }
    }
}
