namespace UncannyValet.Cars
{
    /// <summary>The numbers <see cref="DriftModel"/> needs (docs/redesign/car_handling_spec.md section 2). Speeds are units per second.</summary>
    public readonly struct DriftSettings
    {
        /// <summary>Sideways decay rate at full grip, per second.</summary>
        public readonly float LateralFriction;
        /// <summary>Share (0..1) of the removed sideways speed handed back along the direction of travel.</summary>
        public readonly float ForwardGrip;
        /// <summary>Speed up to which grip is 1.</summary>
        public readonly float GripLowSpeed;
        /// <summary>Speed from which grip equals <see cref="GripAtSpeed"/>.</summary>
        public readonly float GripHighSpeed;
        /// <summary>Grip at and above <see cref="GripHighSpeed"/>.</summary>
        public readonly float GripAtSpeed;
        /// <summary>Grip multiplier while the handbrake is held.</summary>
        public readonly float DriftGripScale;
        /// <summary>Speed above which the handbrake has an effect.</summary>
        public readonly float DriftMinSpeed;

        public DriftSettings(float lateralFriction, float forwardGrip, float gripLowSpeed, float gripHighSpeed,
            float gripAtSpeed, float driftGripScale, float driftMinSpeed)
        {
            LateralFriction = lateralFriction;
            ForwardGrip = forwardGrip;
            GripLowSpeed = gripLowSpeed;
            GripHighSpeed = gripHighSpeed;
            GripAtSpeed = gripAtSpeed;
            DriftGripScale = driftGripScale;
            DriftMinSpeed = driftMinSpeed;
        }
    }
}
