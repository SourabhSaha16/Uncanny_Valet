using System;

namespace UncannyValet.Night
{
    /// <summary>Turns minute-of-night into a 24-hour clock string such as "23:45". Wraps past midnight.</summary>
    public static class ClockFormat
    {
        public static string Format(float minuteOfNight, int startHour = 22)
        {
            var total = startHour * 60 + (int)Math.Floor(Math.Max(0f, minuteOfNight));
            total %= 24 * 60;
            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}
