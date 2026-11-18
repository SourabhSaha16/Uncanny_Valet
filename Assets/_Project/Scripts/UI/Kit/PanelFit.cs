using System;

namespace UncannyValet.UI.Kit
{
    /// <summary>Pure sizing for fixed-width IMGUI panels so they still fit when the window is narrow. Pure.</summary>
    public static class PanelFit
    {
        /// <summary>The wanted width, shrunk so the panel and a margin on each side stay inside the screen.</summary>
        public static float Width(float screenWidth, float wanted, float margin)
        {
            return Math.Max(0f, Math.Min(wanted, screenWidth - margin * 2f));
        }
    }
}
