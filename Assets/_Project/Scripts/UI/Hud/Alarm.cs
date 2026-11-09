using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure alarm rules: when a clock or a patience bar turns amber, then orange-red.</summary>
    public static class Alarm
    {
        /// <summary>The shift clock turns amber with this many seconds left (the final stretch).</summary>
        public const int ClockLowSeconds = 30;
        /// <summary>The shift clock turns orange-red with this many seconds left.</summary>
        public const int ClockDangerSeconds = 10;
        /// <summary>A patience timer turns amber once this fraction of it has run out.</summary>
        public const float PatienceLowElapsed = 0.6f;
        /// <summary>A patience timer turns orange-red once this fraction of it has run out.</summary>
        public const float PatienceDangerElapsed = 0.85f;

        public static AlarmLevel ForClock(int secondsLeft)
        {
            if (secondsLeft <= ClockDangerSeconds) return AlarmLevel.Danger;
            return secondsLeft <= ClockLowSeconds ? AlarmLevel.Low : AlarmLevel.Calm;
        }

        public static AlarmLevel ForPatience(float elapsed01, bool expired)
        {
            if (expired || elapsed01 >= PatienceDangerElapsed) return AlarmLevel.Danger;
            return elapsed01 >= PatienceLowElapsed ? AlarmLevel.Low : AlarmLevel.Calm;
        }

        /// <summary>Pixels of a bar that is still full of patience: 0 = just started (full), 1 = out of time (empty).</summary>
        public static int PatienceFill(float elapsed01, int widthPixels)
        {
            var clamped = Math.Min(1f, Math.Max(0f, elapsed01));
            return (int)((1f - clamped) * widthPixels + 0.5f);
        }
    }
}
