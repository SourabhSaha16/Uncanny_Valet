namespace UncannyValet.Cars
{
    /// <summary>The longitudinal numbers of one car after the live scales: what <see cref="CarPhysicsMath"/> needs.</summary>
    public readonly struct DriveSettings
    {
        /// <summary>Top forward speed, units per second.</summary>
        public readonly float TopSpeed;
        /// <summary>Speed gained per second under throttle.</summary>
        public readonly float Acceleration;
        /// <summary>Deceleration when throttle opposes motion.</summary>
        public readonly float Braking;
        /// <summary>Deceleration with no throttle.</summary>
        public readonly float CoastDrag;
        /// <summary>Reverse top speed as a fraction of the top speed, 0..1.</summary>
        public readonly float ReverseFactor;

        public DriveSettings(float topSpeed, float acceleration, float braking, float coastDrag, float reverseFactor)
        {
            TopSpeed = topSpeed;
            Acceleration = acceleration;
            Braking = braking;
            CoastDrag = coastDrag;
            ReverseFactor = reverseFactor;
        }
    }
}
