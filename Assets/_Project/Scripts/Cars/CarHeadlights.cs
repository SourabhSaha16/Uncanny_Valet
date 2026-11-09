using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Adds two bright headlight dots, each with a soft warm glow behind it, at the car's nose (+Y) so the player can
    /// read which way the car faces. Children of the car root, so they rotate with it and sit above the body sprite.
    /// Built once per car from the shared 1x1 sprite; nothing is updated per frame.
    /// </summary>
    public static class CarHeadlights
    {
        public static void Attach(GameObject car, Vector2 carSize, CarFlashConfig style, Sprite sprite)
        {
            if (!style.HeadlightsEnabled) return;
            Add(car, carSize, style, sprite, -1);
            Add(car, carSize, style, sprite, 1);
        }

        private static void Add(GameObject car, Vector2 carSize, CarFlashConfig style, Sprite sprite, int side)
        {
            var suffix = side < 0 ? "Left" : "Right";
            var glowSize = style.HeadlightSize * style.HeadlightGlowScale;
            Make(car, "HeadlightGlow" + suffix, sprite, carSize, glowSize, style, side, style.HeadlightGlowColor, CarSorting.Glow);
            Make(car, "Headlight" + suffix, sprite, carSize, style.HeadlightSize, style, side, style.HeadlightColor, CarSorting.Headlight);
        }

        private static void Make(GameObject car, string objectName, Sprite sprite, Vector2 carSize, Vector2 size,
            CarFlashConfig style, int side, Color color, int order)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(car.transform, false);
            // The glow is centred on the light, so grow it around the same centre as the light itself.
            var centre = CarHeadlightMath.LocalCentre(carSize, style.HeadlightSize, style.HeadlightFrontInset, style.HeadlightSideInset, side);
            var scale = CarHeadlightMath.LocalScale(carSize, size);
            go.transform.localPosition = new Vector3(centre.x, centre.y, 0f);
            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
        }
    }
}
