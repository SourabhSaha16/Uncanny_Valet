using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// The ground and the walls. The south wall has an opening (the entry gap) where cars drive in from the entry road to
    /// the queue column; the top wall has an opening (the exit gap) above the curb where delivered cars leave.
    /// <see cref="Apply"/> sizes everything without creating objects.
    /// </summary>
    public sealed class LotFrame
    {
        private static readonly string[] WallNames = { "Wall NW", "Wall NE", "Wall SW", "Wall SE", "Wall W", "Wall E" };

        private readonly Transform _ground;
        private readonly Transform[] _walls = new Transform[WallNames.Length];
        private readonly float _thickness;

        public LotFrame(RectFactory rects, Transform parent, float thickness, Color ground, Color wall)
        {
            _thickness = thickness;
            _ground = rects.Make(parent, "Ground", Vector2.zero, Vector2.one, ground, -10).transform;
            for (var i = 0; i < _walls.Length; i++)
            {
                var go = rects.Make(parent, WallNames[i], Vector2.zero, Vector2.one, wall, 0);
                go.AddComponent<BoxCollider2D>().size = Vector2.one;
                _walls[i] = go.transform;
            }
        }

        /// <summary>
        /// Puts the walls just outside <paramref name="b"/>, leaves the south wall open between the entry gap's x values and the
        /// top wall open between the exit gap's x values, and stretches the ground over both.
        /// </summary>
        public void Apply(Rect b, Vector2 entryGap, Vector2 exitGap)
        {
            var t = _thickness;
            var southY = b.yMin - t * 0.5f;
            var northY = b.yMax + t * 0.5f;
            var left = b.xMin - t;
            var right = b.xMax + t;
            Place(_ground, b.center, b.size);
            PlaceSplit(_walls[0], _walls[1], left, right, exitGap, northY, t);
            PlaceSplit(_walls[2], _walls[3], left, right, entryGap, southY, t);
            Place(_walls[4], new Vector2(b.xMin - t * 0.5f, b.center.y), new Vector2(t, b.height));
            Place(_walls[5], new Vector2(b.xMax + t * 0.5f, b.center.y), new Vector2(t, b.height));
        }

        // A horizontal wall from left to right with an opening between gap.x and gap.y, in two pieces.
        private static void PlaceSplit(Transform first, Transform second, float left, float right, Vector2 gap, float y, float thickness)
        {
            var gapLeft = Mathf.Clamp(gap.x, left, right);
            var gapRight = Mathf.Clamp(gap.y, gapLeft, right);
            PlaceSpan(first, left, gapLeft, y, thickness);
            PlaceSpan(second, gapRight, right, y, thickness);
        }

        private static void PlaceSpan(Transform wall, float xMin, float xMax, float y, float thickness)
            => Place(wall, new Vector2((xMin + xMax) * 0.5f, y), new Vector2(xMax - xMin, thickness));

        private static void Place(Transform t, Vector2 center, Vector2 size)
        {
            t.localPosition = center;
            t.localScale = new Vector3(size.x, size.y, 1f);
        }
    }
}
