using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Lays out sprites as a grid of renderers filling a rectangle, whatever the sprite pivot is.</summary>
    public static class SpriteTiler
    {
        private const float CellTolerance = 0.001f;

        /// <summary>Fills <paramref name="area"/> (parent-local) with whole tiles; the last row and column may overshoot.</summary>
        public static Transform Fill(Transform parent, string name, Sprite tile, Rect area, int order)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            if (tile == null) return group;
            var cell = tile.rect.size / tile.pixelsPerUnit;
            var pivot = tile.pivot / tile.pixelsPerUnit;
            var columns = Mathf.CeilToInt(area.width / cell.x - CellTolerance);
            var rows = Mathf.CeilToInt(area.height / cell.y - CellTolerance);
            for (var r = 0; r < rows; r++)
            for (var c = 0; c < columns; c++)
            {
                var go = new GameObject(name + " " + r + "," + c);
                go.transform.SetParent(group, false);
                go.transform.localPosition = new Vector3(area.xMin + c * cell.x + pivot.x, area.yMin + r * cell.y + pivot.y, 0f);
                Place(go.AddComponent<SpriteRenderer>(), tile, order);
            }
            return group;
        }

        /// <summary>One sprite whose pivot sits at <paramref name="position"/> (parent-local).</summary>
        public static void Single(Transform parent, string name, Sprite sprite, Vector2 position, int order)
        {
            if (sprite == null) return;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            Place(go.AddComponent<SpriteRenderer>(), sprite, order);
        }

        private static void Place(SpriteRenderer renderer, Sprite sprite, int order)
        {
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
        }
    }
}
