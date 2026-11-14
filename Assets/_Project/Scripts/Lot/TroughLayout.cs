using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure placement of the wash troughs, which stand outside the bay grid. With no extension a single trough sits beside the bay block in the
    /// top row (in the open ground before the right strip); with an extension they are stacked at the far end, the first in the top row, the
    /// second in the bottom row and the rest below it, all a fixed distance east of the bay block. Each trough is as big as a bay.
    /// </summary>
    public static class TroughLayout
    {
        /// <summary>The trough rectangles in lot space, first to last (top to bottom).</summary>
        /// <param name="curbTop">Top edge of the curb, which is also the top edge of the top bay row.</param>
        /// <param name="baysRight">Right edge of the last bay column.</param>
        public static Rect[] Place(LotConfig config, LotLayoutOptions options, float curbTop, float baysRight)
        {
            var rects = new Rect[options.WashTroughs];
            var size = config.SpotSize;
            var far = options.ExtensionWidth > 0f;
            var x = far ? baysRight + options.TroughDistance + size.x * 0.5f : baysRight + config.Site.StripGap * 0.5f;
            var y = curbTop - size.y * 0.5f;
            for (var i = 0; i < rects.Length; i++)
            {
                rects[i] = LotGeometry.Box(new Vector2(x, y), size);
                y -= size.y + (i == 0 ? config.AisleWidth : config.Site.TroughStackGap);
            }
            return rects;
        }
    }
}
