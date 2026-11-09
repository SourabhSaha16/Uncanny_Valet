using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Draws the car's top-down sprite as a child "Body" that rotates with the car. Regular and EV sprites are
    /// picked from the car id once; <see cref="SetEv"/> swaps between them. Never touches materials.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarBodyArt : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private CarSpriteEntry _regular;
        private CarSpriteEntry _ev;
        private Vector2 _carSize;

        /// <summary>Adds the body child to <paramref name="car"/> showing the regular sprite.</summary>
        public static CarBodyArt Attach(GameObject car, CarArtConfig art, CarId id, Vector2 carSize)
        {
            var go = new GameObject("Body");
            go.transform.SetParent(car.transform, false);
            var body = go.AddComponent<CarBodyArt>();
            body._renderer = go.AddComponent<SpriteRenderer>();
            body._renderer.sortingOrder = CarSorting.Body;
            body._carSize = carSize;
            body._regular = art.PickCar(id.Value);
            body._ev = art.PickEv(id.Value);
            body.Show(body._regular);
            return body;
        }

        /// <summary>Shows the EV sprite (when one exists) or the regular one.</summary>
        public void SetEv(bool isEv) => Show(isEv && _ev != null ? _ev : _regular);

        private void Show(CarSpriteEntry entry)
        {
            if (entry == null || entry.Sprite == null)
            {
                Debug.LogError("CarBodyArt on " + transform.parent.name + " has no sprite; check the CarArtConfig.", this);
                return;
            }
            if (!CarArtMath.IsHalfTurn(entry.RotationOffsetDegrees))
                Debug.LogError("Car sprite rotation offset must be 0 or 180: " + entry.Sprite.name, this);
            var uniform = CarArtMath.UniformScale(entry.ContentSize, _carSize);
            var scale = CarArtMath.LocalScale(uniform, _carSize);
            var offset = CarArtMath.LocalOffset(entry.ContentCenter, uniform, _carSize, entry.RotationOffsetDegrees);
            _renderer.sprite = entry.Sprite;
            transform.localScale = new Vector3(scale.x, scale.y, 1f);
            transform.localPosition = new Vector3(offset.x, offset.y, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, entry.RotationOffsetDegrees);
        }
    }
}
