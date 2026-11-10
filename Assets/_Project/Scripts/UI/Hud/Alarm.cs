namespace UncannyValet.UI.Hud
{
    /// <summary>Pure alarm rules: when the clock turns amber, then orange-red.</summary>
    public static class Alarm
    {
        /// <summary>The shift clock turns amber with this many seconds left (the final stretch).</summary>
        public const int ClockLowSeconds = 30;
        /// <summary>The shift clock turns orange-red with this many seconds left.</summary>
        public const int ClockDangerSeconds = 10;

        public static AlarmLevel ForClock(int secondsLeft)
        {
            if (secondsLeft <= ClockDangerSeconds) return AlarmLevel.Danger;
            return secondsLeft <= ClockLowSeconds ? AlarmLevel.Low : AlarmLevel.Calm;
        }
    }
}
