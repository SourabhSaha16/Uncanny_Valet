namespace UncannyValet.Cars
{
    /// <summary>The numbers <see cref="DriftState"/> needs: the entry kick, the turn boost and the grip hold and blend (issue 151).</summary>
    public readonly struct DriftKickSettings
    {
        /// <summary>Yaw rate (radians per second) added at the instant the drift button starts, fading over <see cref="KickSeconds"/>.</summary>
        public readonly float KickRate;
        /// <summary>Seconds the kick takes to fade to zero.</summary>
        public readonly float KickSeconds;
        /// <summary>Turn rate multiplier while drifting.</summary>
        public readonly float TurnScale;
        /// <summary>Seconds the drift grip is kept after the button is released.</summary>
        public readonly float HoldSeconds;
        /// <summary>Seconds grip takes to blend back to normal after the hold.</summary>
        public readonly float BlendSeconds;
        /// <summary>Speed above which the drift button has an effect.</summary>
        public readonly float MinSpeed;

        public DriftKickSettings(float kickRate, float kickSeconds, float turnScale, float holdSeconds, float blendSeconds, float minSpeed)
        {
            KickRate = kickRate;
            KickSeconds = kickSeconds;
            TurnScale = turnScale;
            HoldSeconds = holdSeconds;
            BlendSeconds = blendSeconds;
            MinSpeed = minSpeed;
        }
    }
}
