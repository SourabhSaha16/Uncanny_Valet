using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure placement of zebra crossing bars: bars as long as the crossing is wide, laid one after the other along the
    /// walking direction with a gap between them, centred in the area.
    /// </summary>
    public static class ZebraStripes
    {
        /// <summary>True when people walk along x in <paramref name="area"/> (it is at least as wide as it is tall).</summary>
        public static bool WalksAlongX(Rect area) => area.width >= area.height;

        /// <summary>How many bars of <paramref name="bar"/> with <paramref name="gap"/> between them fit a run of <paramref name="length"/>.</summary>
        public static int Count(float length, float bar, float gap)
            => Mathf.Max(0, Mathf.FloorToInt((length + gap) / Mathf.Max(0.001f, bar + gap)));

        /// <summary>
        /// The bars of one crossing. Each bar spans the area across the walking direction, less <paramref name="margin"/> at both
        /// ends, and is <paramref name="bar"/> long along it.
        /// </summary>
        public static Rect[] Bars(Rect area, float bar, float gap, float margin)
        {
            var alongX = WalksAlongX(area);
            var length = alongX ? area.width : area.height;
            var across = Mathf.Max(0.01f, (alongX ? area.height : area.width) - margin * 2f);
            var count = Count(length, bar, gap);
            var bars = new Rect[count];
            var start = (alongX ? area.xMin : area.yMin) + (length - (count * bar + Mathf.Max(0, count - 1) * gap)) * 0.5f;
            for (var i = 0; i < count; i++)
            {
                var from = start + i * (bar + gap);
                bars[i] = alongX
                    ? new Rect(from, area.center.y - across * 0.5f, bar, across)
                    : new Rect(area.center.x - across * 0.5f, from, across, bar);
            }
            return bars;
        }
    }
}
