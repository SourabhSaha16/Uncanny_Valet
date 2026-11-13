using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>Colours of the power bar: gold bolts and fill, one colour per power icon.</summary>
    public static class PowerColors
    {
        private const float DimFactor = 0.45f;

        public static readonly Color Gold = new Color32(0xFF, 0xC8, 0x3A, 255);
        public static readonly Color IceBlue = new Color32(0x8A, 0xDC, 0xFF, 255);

        public static readonly Color StormGreen = new Color32(0x9A, 0xE8, 0x9A, 255);
        public static readonly Color Silver = new Color32(0xD8, 0xDC, 0xE6, 255);
        public static readonly Color CooldownGrey = new Color32(0x8C, 0x90, 0x9C, 255);

        /// <summary>The icon colour of a story power.</summary>
        public static Color Of(UncannyValet.Core.Ids.StoryPower power)
        {
            switch (power)
            {
                case UncannyValet.Core.Ids.StoryPower.ThunderStride: return Gold;
                case UncannyValet.Core.Ids.StoryPower.StormWind: return StormGreen;
                case UncannyValet.Core.Ids.StoryPower.FreezeTime: return IceBlue;
                default: return Silver;
            }
        }

        /// <summary>The letter drawn on the icon of a story power.</summary>
        public static char LetterOf(UncannyValet.Core.Ids.StoryPower power)
        {
            switch (power)
            {
                case UncannyValet.Core.Ids.StoryPower.ThunderStride: return 'T';
                case UncannyValet.Core.Ids.StoryPower.StormWind: return 'S';
                case UncannyValet.Core.Ids.StoryPower.FreezeTime: return 'F';
                default: return 'M';
            }
        }

        /// <summary>A colour darkened, for a bolt that is not open yet.</summary>
        public static Color Dimmed(Color color) => new Color(color.r * DimFactor, color.g * DimFactor, color.b * DimFactor, 1f);
    }
}
