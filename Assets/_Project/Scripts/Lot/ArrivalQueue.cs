using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure maths for the arrival queue: car-sized slots side by side along a direction perpendicular to the
    /// cars' length (like adjacent bays). Slot 0 is the first point; each next slot is one pitch further along.
    /// </summary>
    public static class ArrivalQueue
    {
        /// <summary>Centre-to-centre distance for cars of the given width with a gap between their sides.</summary>
        public static float Pitch(float carWidth, float gap) => carWidth + Mathf.Max(0f, gap);

        public static Vector2[] Slots(Vector2 first, int count, Vector2 direction, float pitch)
        {
            var dir = direction.sqrMagnitude < 0.0001f ? Vector2.left : direction.normalized;
            var slots = new Vector2[Mathf.Max(1, count)];
            for (var i = 0; i < slots.Length; i++) slots[i] = first + dir * (pitch * i);
            return slots;
        }

        /// <summary>Axis-aligned box around a car-sized rectangle rotated by <paramref name="degrees"/>.</summary>
        public static Rect Footprint(Vector2 center, Vector2 size, float degrees)
        {
            var rad = degrees * Mathf.Deg2Rad;
            var c = Mathf.Abs(Mathf.Cos(rad));
            var s = Mathf.Abs(Mathf.Sin(rad));
            var extent = new Vector2(size.x * c + size.y * s, size.x * s + size.y * c);
            return new Rect(center - extent * 0.5f, extent);
        }

        /// <summary>Union of the footprints of every slot.</summary>
        public static Rect Bounds(Vector2[] slots, Vector2 carSize, float degrees)
        {
            var all = Footprint(slots[0], carSize, degrees);
            for (var i = 1; i < slots.Length; i++)
            {
                var r = Footprint(slots[i], carSize, degrees);
                all = Rect.MinMaxRect(Mathf.Min(all.xMin, r.xMin), Mathf.Min(all.yMin, r.yMin),
                    Mathf.Max(all.xMax, r.xMax), Mathf.Max(all.yMax, r.yMax));
            }
            return all;
        }
    }
}
