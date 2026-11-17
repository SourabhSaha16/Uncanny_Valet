using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure placement of the wash bays, which stand outside the bay grid: one column right beside the bay block, a small
    /// <see cref="LotLayoutOptions.WashBayGap"/> east of it, the first in the top row, the second in the bottom row and the rest below it.
    /// Each wash bay is as big as a bay.
    /// </summary>
    public static class WashBayLayout
    {
        /// <summary>The wash bay rectangles in lot space, first to last (top to bottom).</summary>
        /// <param name="curbTop">Top edge of the curb, which is also the top edge of the top bay row.</param>
        /// <param name="baysRight">Right edge of the last bay column.</param>
        public static Rect[] Place(LotConfig config, LotLayoutOptions options, float curbTop, float baysRight)
        {
            var rects = new Rect[options.WashTroughs];
            var size = config.SpotSize;
            var x = baysRight + options.WashBayGap + size.x * 0.5f;
            var y = curbTop - size.y * 0.5f;
            for (var i = 0; i < rects.Length; i++)
            {
                rects[i] = LotGeometry.Box(new Vector2(x, y), size);
                y -= size.y + (i == 0 ? config.AisleWidth + config.BottomRowDrop : config.Site.BayStackGap);
            }
            return rects;
        }
    }
}
