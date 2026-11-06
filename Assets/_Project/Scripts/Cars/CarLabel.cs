using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Centred ticket-letter label on a car (child object, so it rotates with the car).</summary>
    [DisallowMultipleComponent]
    public sealed class CarLabel : MonoBehaviour
    {
        private CarId _id;
        private TextMesh _text;
        private IEventBus _bus;

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
            text.text = string.Empty;
            text.fontSize = CarLabelMath.FontSize;
            text.characterSize = CarLabelMath.CharacterSizeFor(
                CarLabelMath.TargetHeight(carSize.x, heightFraction));
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = color;

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material;
            renderer.sortingOrder = CarSorting.Label;

            var label = go.AddComponent<CarLabel>();
            label._id = id;
            label._text = text;
        }

        private void OnEnable()
        {
            if (!Services.TryGet<IEventBus>(out _bus)) return;
            _bus.Subscribe<CarLetterAssigned>(OnLetterAssigned);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<CarLetterAssigned>(OnLetterAssigned);
            _bus = null;
        }

        private void OnLetterAssigned(CarLetterAssigned e)
        {
            if (e.Car == _id) _text.text = CarLabelMath.LetterText(e.Letter);
        }
    }
}
