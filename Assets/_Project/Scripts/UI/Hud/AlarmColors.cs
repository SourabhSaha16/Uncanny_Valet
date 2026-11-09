using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>Maps an <see cref="AlarmLevel"/> to the style guide's colours: amber for low, orange-red for danger.</summary>
    public static class AlarmColors
    {
        public static Color For(AlarmLevel level, Color calm)
        {
            if (level == AlarmLevel.Danger) return PixelPalette.OrangeRed;
            return level == AlarmLevel.Low ? PixelPalette.Amber : calm;
        }
    }
}
