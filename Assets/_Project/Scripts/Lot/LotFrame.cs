using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// The ground and the walls. The south wall has an opening (the entry gap) where cars drive in from the road to the
    /// queue column and out of the corridor. <see cref="Apply"/> sizes everything without creating objects.
    /// </summary>
    public sealed class LotFrame
    {
        private static readonly string[] WallNames = { "Wall N", "Wall SW", "Wall SE", "Wall W", "Wall E" };

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

        /// <summary>Puts the walls just outside <paramref name="b"/>, leaves the south wall open between the gap's x values and stretches the ground over it.</summary>
        public void Apply(Rect b, Vector2 entryGap)
        {
            var t = _thickness;
            var southY = b.yMin - t * 0.5f;
            var gapLeft = Mathf.Clamp(entryGap.x, b.xMin - t, b.xMax + t);
            var gapRight = Mathf.Clamp(entryGap.y, gapLeft, b.xMax + t);
            Place(_ground, b.center, b.size);
            Place(_walls[0], new Vector2(b.center.x, b.yMax + t * 0.5f), new Vector2(b.width + t * 2f, t));
            PlaceSpan(_walls[1], b.xMin - t, gapLeft, southY, t);
            PlaceSpan(_walls[2], gapRight, b.xMax + t, southY, t);
            Place(_walls[3], new Vector2(b.xMin - t * 0.5f, b.center.y), new Vector2(t, b.height));
            Place(_walls[4], new Vector2(b.xMax + t * 0.5f, b.center.y), new Vector2(t, b.height));
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
