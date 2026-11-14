using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Paints the arrival queue as a column of dotted outline boxes, one per waiting car slot (no arrows). The first box is the
    /// valet pickup spot: its outline is drawn in the pickup colour and it carries the pickup label. Purely visual.
    /// </summary>
    public static class LotQueueArt
    {
        private const int Order = -5;
        private const int PickupSlot = 0;

        /// <summary>One dotted box inside every slot footprint in <paramref name="slots"/>.</summary>
        public static void Build(Transform parent, RectFactory rects, Rect[] slots, WalkwayStyle style, Color color, LotConfig config)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                var box = Inset(slots[i], style.SlotInset);
                var dashes = DashedOutline.Dashes(box, style.SlotLine, style.SlotDash, style.SlotGap);
                var paint = i == PickupSlot ? config.PickupSlotColor : color;
                for (var d = 0; d < dashes.Count; d++)
                    rects.Make(parent, "Queue Slot " + i + " Dash " + d, dashes[d].center, dashes[d].size, paint, Order);
            }
            if (slots.Length > 0) BuildPickupLabel(parent, slots[PickupSlot], config);
        }

        // The words are stacked vertically in columns, like the curb text, so they fit the slot's width.
        private static void BuildPickupLabel(Transform parent, Rect slot, LotConfig config)
        {
            var words = config.PickupLabelWords;
            for (var i = 0; i < words.Length; i++)
            {
                var x = StackedText.ColumnOffset(i, words.Length, config.PickupLabelColumnSpacing);
                SpotLabel.Create(parent, StackedText.Stack(words[i]), slot.center + new Vector2(x, 0f), config, Order, config.PickupLabelSize);
            }
        }

        private static Rect Inset(Rect r, float by) => Rect.MinMaxRect(r.xMin + by, r.yMin + by, r.xMax - by, r.yMax - by);
    }
}
