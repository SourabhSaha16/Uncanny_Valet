namespace UncannyValet.Cars.Fx
{
    /// <summary>What <see cref="SkidRule"/> decided for one step.</summary>
    public readonly struct SkidState
    {
        /// <summary>True while the car is sliding.</summary>
        public readonly bool Sliding;
        /// <summary>True on the step the slide began.</summary>
        public readonly bool SlideStarted;
        /// <summary>True on the step the slide ended.</summary>
        public readonly bool SlideEnded;
        /// <summary>True on the step hard braking began.</summary>
        public readonly bool HardBrakeStarted;
        /// <summary>Slide strength 0..1 from the slip angle; 0 when not sliding.</summary>
        public readonly float Intensity01;
        /// <summary>Mark opacity 0..max; 0 when not sliding.</summary>
        public readonly float Opacity;

        public SkidState(bool sliding, bool started, bool ended, bool hardBrakeStarted, float intensity01, float opacity)
        {
            Sliding = sliding;
            SlideStarted = started;
            SlideEnded = ended;
            HardBrakeStarted = hardBrakeStarted;
            Intensity01 = intensity01;
            Opacity = opacity;
        }
    }
}
