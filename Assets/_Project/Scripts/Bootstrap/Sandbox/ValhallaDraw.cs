using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>Places sprite renderers for the Valhalla lot: tiled floors, sprites stretched to a rectangle, and sprites sized by height. Built once, never per frame.</summary>
    public static class ValhallaDraw
    {
        private const float QuarterTurn = 45f;

        private static SpriteRenderer Make(Transform parent, string name, Sprite sprite, Vector2 center, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }

        /// <summary>Repeats the sprite over <paramref name="area"/>; <paramref name="tileScale"/> sizes one repeat (1 = the sprite's own size).</summary>
        public static void Tiled(Transform parent, string name, Sprite sprite, Rect area, int order, float tileScale = 1f)
        {
            var renderer = Make(parent, name, sprite, area.center, order);
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = area.size / tileScale;
            renderer.transform.localScale = new Vector3(tileScale, tileScale, 1f);
        }

        /// <summary>Stretches the sprite over <paramref name="area"/>. A quarter turn (90 or -90) swaps the sprite's axes, so the area is the footprint after turning.</summary>
        public static void Fit(Transform parent, string name, Sprite sprite, Rect area, int order, float degrees = 0f)
        {
            var renderer = Make(parent, name, sprite, area.center, order);
            var size = sprite.bounds.size;
            var turned = Mathf.Abs(degrees) > QuarterTurn;
            var wide = turned ? area.height : area.width;
            var tall = turned ? area.width : area.height;
            renderer.transform.localScale = new Vector3(wide / size.x, tall / size.y, 1f);
            renderer.transform.localRotation = Quaternion.Euler(0f, 0f, degrees);
        }

        /// <summary>Draws the sprite at <paramref name="center"/> with its height set to <paramref name="height"/> world units, keeping its proportions.</summary>
        public static void Stand(Transform parent, string name, Sprite sprite, Vector2 center, float height, int order, float degrees = 0f)
        {
            var renderer = Make(parent, name, sprite, center, order);
            var scale = height / sprite.bounds.size.y;
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
            renderer.transform.localRotation = Quaternion.Euler(0f, 0f, degrees);
        }
    }
}
