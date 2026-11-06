using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Creates a world-space text label (TextMesh) centred on a point. WebGL safe.</summary>
    public static class SpotLabel
    {
        public static void Create(Transform parent, string text, Vector2 center, LotConfig config, int order)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go = new GameObject("Label " + text);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(center.x, center.y, 0f);

            var mesh = go.AddComponent<TextMesh>();
            mesh.font = font;
            mesh.text = text;
            mesh.fontSize = 64;
            mesh.characterSize = config.LabelSize / 6.4f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = config.LabelColor;

            var rend = go.GetComponent<MeshRenderer>();
            rend.sharedMaterial = font.material;
            rend.sortingOrder = order;
        }
    }
}
