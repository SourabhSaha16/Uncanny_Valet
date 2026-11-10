namespace UncannyValet.Traffic
{
    /// <summary>The numbers car following needs. Speeds are world units per second, gaps are bumper to bumper.</summary>
    public readonly struct FollowSettings
    {
        /// <summary>Speed the car wants when the road is clear.</summary>
        public readonly float DesiredSpeed;
        /// <summary>Gap kept when standing still behind a leader.</summary>
        public readonly float MinGap;
        /// <summary>Extra gap per unit of speed, expressed as seconds.</summary>
        public readonly float Headway;
        /// <summary>Speed gained per second.</summary>
        public readonly float Acceleration;
        /// <summary>Speed lost per second when braking.</summary>
        public readonly float Brake;

        public FollowSettings(float desiredSpeed, float minGap, float headway, float acceleration, float brake)
        {
            DesiredSpeed = desiredSpeed;
            MinGap = minGap;
            Headway = headway;
            Acceleration = acceleration;
            Brake = brake;
        }

        /// <summary>The same settings with another desired speed.</summary>
        public FollowSettings WithDesiredSpeed(float desiredSpeed) =>
            new FollowSettings(desiredSpeed, MinGap, Headway, Acceleration, Brake);
    }
}
