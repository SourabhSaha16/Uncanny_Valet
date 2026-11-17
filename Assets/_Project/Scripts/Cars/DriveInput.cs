namespace UncannyValet.Cars
{
    /// <summary>
    /// One frame of driver input: throttle and steer in -1..1 and the handbrake, plus the scales a modifier may put on the
    /// car's top speed, acceleration and grip loss (a plain input leaves all at 1; a boost raises the first two, mud lowers all three).
    /// </summary>
    public readonly struct DriveInput
    {
        public readonly float Throttle;
        public readonly float Steer;
        public readonly bool Drift;
        private readonly float _topSpeedExtra;
        private readonly float _accelerationExtra;
        private readonly float _gripLossExtra;

        public DriveInput(float throttle, float steer, bool drift, float topSpeedScale = 1f, float accelerationScale = 1f,
            float gripLossScale = 1f)
        {
            Throttle = throttle;
            Steer = steer;
            Drift = drift;
            _topSpeedExtra = topSpeedScale - 1f;
            _accelerationExtra = accelerationScale - 1f;
            _gripLossExtra = gripLossScale - 1f;
        }

        /// <summary>Multiplier on the car's top speed (1 = unchanged).</summary>
        public float TopSpeedScale => 1f + _topSpeedExtra;

        /// <summary>Multiplier on the car's acceleration (1 = unchanged).</summary>
        public float AccelerationScale => 1f + _accelerationExtra;

        /// <summary>Multiplier on the grip the car loses to speed and the handbrake (1 = unchanged, 0.4 = 40% of the loss).</summary>
        public float GripLossScale => 1f + _gripLossExtra;

        /// <summary>The same input with the top speed and acceleration scales multiplied by the given factors.</summary>
        public DriveInput Scaled(float topSpeedFactor, float accelerationFactor) =>
            new DriveInput(Throttle, Steer, Drift, TopSpeedScale * topSpeedFactor, AccelerationScale * accelerationFactor, GripLossScale);

        /// <summary>The same input with the grip loss scale multiplied by <paramref name="factor"/>.</summary>
        public DriveInput WithGripLossScaled(float factor) =>
            new DriveInput(Throttle, Steer, Drift, TopSpeedScale, AccelerationScale, GripLossScale * factor);
    }
}
