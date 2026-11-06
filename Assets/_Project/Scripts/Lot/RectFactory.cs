using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Makes coloured rectangle GameObjects from one shared 1x1 white sprite.</summary>
    public sealed class RectFactory
    {
        private readonly Sprite _sprite;

        public RectFactory()
        {
            var tex = new Texture2D(1, 1) { filterMode = FilterMode.Point };
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        public GameObject Make(Transform parent, string name, Vector2 center, Vector2 size, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return go;
        }

        /// <summary>A line of the given thickness from <paramref name="a"/> to <paramref name="b"/> (local to the parent).</summary>
        public GameObject Segment(Transform parent, string name, Vector2 a, Vector2 b, float thickness, Color color, int order)
        {
            var d = b - a;
            var go = Make(parent, name, (a + b) * 0.5f, new Vector2(d.magnitude, thickness), color, order);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            return go;
        }
    }
}
