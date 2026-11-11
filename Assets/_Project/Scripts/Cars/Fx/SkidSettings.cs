namespace UncannyValet.Cars.Fx
{
    /// <summary>The numbers <see cref="SkidRule"/> needs: the live ones from the F2 panel plus the fixed ones of <see cref="SkidConfig"/>.</summary>
    public readonly struct SkidSettings
    {
        /// <summary>Sideways speed above which the car slides, units per second.</summary>
        public readonly float SlideThreshold;
        /// <summary>Speed above which holding the drift button alone counts as sliding.</summary>
        public readonly float ShiftSlideSpeed;
        /// <summary>Opacity of a full-drift mark, 0..1.</summary>
        public readonly float MaxOpacity;
        /// <summary>A slide only ends below this share of its start threshold.</summary>
        public readonly float ExitShare;
        /// <summary>Slip angle that counts as a full drift, degrees.</summary>
        public readonly float FullDriftDegrees;
        /// <summary>Opacity of the gentlest slide as a share of <see cref="MaxOpacity"/>.</summary>
        public readonly float FaintShare;
        /// <summary>Speed above which braking is hard braking.</summary>
        public readonly float HardBrakeSpeed;

        public SkidSettings(float slideThreshold, float shiftSlideSpeed, float maxOpacity, float exitShare,
            float fullDriftDegrees, float faintShare, float hardBrakeSpeed)
        {
            SlideThreshold = slideThreshold;
            ShiftSlideSpeed = shiftSlideSpeed;
            MaxOpacity = maxOpacity;
            ExitShare = exitShare;
            FullDriftDegrees = fullDriftDegrees;
            FaintShare = faintShare;
            HardBrakeSpeed = hardBrakeSpeed;
        }
    }
}
