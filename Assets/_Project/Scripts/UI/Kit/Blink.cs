using System;

namespace UncannyValet.UI.Kit
{
    /// <summary>Pure on/off flashing: on for the first half of each period.</summary>
    public static class Blink
    {
        public static bool IsOn(float time, float periodSeconds)
        {
            if (periodSeconds <= 0f) return true;
            var phase = time - periodSeconds * (float)Math.Floor(time / periodSeconds);
            return phase < periodSeconds * 0.5f;
        }
    }
}
