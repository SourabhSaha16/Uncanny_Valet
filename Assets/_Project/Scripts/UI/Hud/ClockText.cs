using System;
using System.Text;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure shift-clock text: m:ss, rounded up so 0:00 means the shift is over.</summary>
    public static class ClockText
    {
        public static int WholeSeconds(float secondsRemaining)
            => (int)Math.Ceiling(Math.Max(0f, secondsRemaining));

        /// <summary>Appends e.g. "1:12" to the builder.</summary>
        public static void Write(StringBuilder text, int totalSeconds)
        {
            var seconds = Math.Max(0, totalSeconds);
            var rest = seconds % 60;
            text.Append(seconds / 60).Append(':');
            if (rest < 10) text.Append('0');
            text.Append(rest);
        }
    }
}
