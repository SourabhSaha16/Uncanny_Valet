using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Paints the arrival queue as a column of dotted outline boxes, one per waiting car slot (no arrows). Purely visual.</summary>
    public static class LotQueueArt
    {
        private const int Order = -5;

        /// <summary>One dotted box inside every slot footprint in <paramref name="slots"/>.</summary>
        public static void Build(Transform parent, RectFactory rects, Rect[] slots, WalkwayStyle style, Color color)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                var box = Inset(slots[i], style.SlotInset);
                var dashes = DashedOutline.Dashes(box, style.SlotLine, style.SlotDash, style.SlotGap);
                for (var d = 0; d < dashes.Count; d++)
                    rects.Make(parent, "Queue Slot " + i + " Dash " + d, dashes[d].center, dashes[d].size, color, Order);
            }
        }

        private static Rect Inset(Rect r, float by) => Rect.MinMaxRect(r.xMin + by, r.yMin + by, r.xMax - by, r.yMax - by);
    }
}
