using System;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.UI.Kit
{
    /// <summary>Maps the icon a modifier definition names to the HUD's pixel icon.</summary>
    public static class ModifierIcons
    {
        public static PixelIcon ToPixel(ModifierIcon icon)
        {
            switch (icon)
            {
                case ModifierIcon.Mud: return PixelIcon.Mud;
                case ModifierIcon.Plug: return PixelIcon.Plug;
                default: throw new ArgumentOutOfRangeException(nameof(icon), icon, "Modifier icon has no pixel icon.");
            }
        }
    }
}
