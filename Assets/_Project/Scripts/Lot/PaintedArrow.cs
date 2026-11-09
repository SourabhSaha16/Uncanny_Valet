using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>A painted arrow on the asphalt: a shaft from tail to tip plus two head strokes.</summary>
    public readonly struct PaintedArrow
    {
        public readonly Vector2 Tail;
        public readonly Vector2 Tip;

        public PaintedArrow(Vector2 tail, Vector2 tip) { Tail = tail; Tip = tip; }

        /// <summary>The far end of one head stroke: from the tip, back along the shaft, turned by <paramref name="degrees"/>.</summary>
        public Vector2 HeadEnd(float length, float degrees)
        {
            var back = (Tail - Tip).normalized;
            var radians = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);
            var turned = new Vector2(back.x * cos - back.y * sin, back.x * sin + back.y * cos);
            return Tip + turned * length;
        }
    }
}
