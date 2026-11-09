using System;

namespace UncannyValet.Night
{
    /// <summary>Turns seconds remaining into a countdown string such as "3:45". Rounds up so 0:00 means over.</summary>
    public static class ClockFormat
    {
        public static string Format(float secondsRemaining)
        {
            var total = (int)Math.Ceiling(Math.Max(0f, secondsRemaining));
            return $"{total / 60}:{total % 60:00}";
        }
    }
}
