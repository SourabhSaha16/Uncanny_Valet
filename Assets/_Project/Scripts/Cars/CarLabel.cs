using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Adds a centred car-number label as a child, so it rotates with the car.</summary>
    public static class CarLabel
    {
        /// <param name="heightFraction">Text height as a fraction of the car's width.</param>
        public static void Attach(GameObject car, CarId id, Vector2 carSize, float heightFraction, Color color)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go = new GameObject("Label");
            go.transform.SetParent(car.transform, false);
            // Cancel the parent's non-uniform scale so the glyphs are not stretched.
            go.transform.localScale = new Vector3(1f / carSize.x, 1f / carSize.y, 1f);

            var text = go.AddComponent<TextMesh>();
            text.font = font;
            text.text = id.Value.ToString();
            text.fontSize = 100;
            text.characterSize = Mathf.Max(0.01f, carSize.x * heightFraction);
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = color;

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material;
            renderer.sortingOrder = 11;
        }
    }
}
