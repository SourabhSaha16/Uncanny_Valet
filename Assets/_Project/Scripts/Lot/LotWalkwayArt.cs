using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Paints every crosswalk of the lot as white zebra bars on the asphalt, from plain rectangles. Purely visual.</summary>
    public static class LotWalkwayArt
    {
        private const int BarOrder = -8;

        /// <summary>Builds the zebra bars of every crosswalk of <paramref name="layout"/> under <paramref name="parent"/>.</summary>
        public static void Build(Transform parent, RectFactory rects, WalkwayLayout layout, WalkwayStyle style, Color color)
        {
            for (var i = 0; i < layout.Crosswalks.Length; i++)
            {
                var bars = ZebraStripes.Bars(layout.Crosswalks[i], style.ZebraBar, style.ZebraGap, style.ZebraMargin);
                for (var b = 0; b < bars.Length; b++)
                    rects.Make(parent, "Crosswalk " + i + " Bar " + b, bars[b].center, bars[b].size, color, BarOrder);
            }
        }
    }
}
