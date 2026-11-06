using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Builds the four boundary walls around a rectangle.</summary>
    public static class WallBuilder
    {
        public static void Build(RectFactory rects, Transform parent, Rect b, float t, Color color)
        {
            Add(rects, parent, "Wall N", new Vector2(b.center.x, b.yMax + t * 0.5f), new Vector2(b.width + t * 2f, t), color);
            Add(rects, parent, "Wall S", new Vector2(b.center.x, b.yMin - t * 0.5f), new Vector2(b.width + t * 2f, t), color);
            Add(rects, parent, "Wall W", new Vector2(b.xMin - t * 0.5f, b.center.y), new Vector2(t, b.height), color);
            Add(rects, parent, "Wall E", new Vector2(b.xMax + t * 0.5f, b.center.y), new Vector2(t, b.height), color);
        }

        private static void Add(RectFactory rects, Transform parent, string name, Vector2 center, Vector2 size, Color color)
        {
            var go = rects.Make(parent, name, center, size, color, 0);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
        }
    }
}
