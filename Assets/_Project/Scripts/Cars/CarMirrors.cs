using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Adds the two side mirrors at the car's front (+Y) as children, using the shared sprite.</summary>
    public static class CarMirrors
    {
        public static void Attach(GameObject car, Vector2 carSize, CarMirrorStyle style, Sprite sprite)
        {
            Add(car, carSize, style, sprite, -1);
            Add(car, carSize, style, sprite, 1);
        }

        private static void Add(GameObject car, Vector2 carSize, CarMirrorStyle style, Sprite sprite, int side)
        {
            var go = new GameObject(side < 0 ? "MirrorLeft" : "MirrorRight");
            go.transform.SetParent(car.transform, false);
            Vector2 centre = CarMirrorMath.LocalCentre(carSize, style.Size, style.FrontInset, style.Outset, side);
            Vector2 scale = CarMirrorMath.LocalScale(carSize, style.Size);
            go.transform.localPosition = new Vector3(centre.x, centre.y, 0f);
            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = style.Color;
            renderer.sortingOrder = CarSorting.Mirrors;
        }
    }
}
