using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>Measures the world rectangle a built lot covers (ground and walls), once.</summary>
    public static class LotBounds
    {
        /// <summary>Returns false when the object has no renderers yet.</summary>
        public static bool TryMeasure(GameObject lot, out Rect rect)
        {
            rect = default;
            var renderers = lot.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return false;
            var b = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            rect = Rect.MinMaxRect(b.min.x, b.min.y, b.max.x, b.max.y);
            return true;
        }
    }
}
