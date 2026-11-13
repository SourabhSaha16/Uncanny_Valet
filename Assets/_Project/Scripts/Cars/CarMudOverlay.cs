using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Dark brown overlay on a Muddy car. Created inactive; CarTraitLook activates it. Washing
    /// (ModifierProgressChanged of a mud-look modifier) wipes it away left to right, revealing the car colour;
    /// ModifierCleared hides it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarMudOverlay : MonoBehaviour
    {
        private CarId _id;
        private CarDefinition _definition;
        private SpriteRenderer _renderer;
        private IEventBus _bus;
        private IModifierCatalog _modifiers;
        private float _shownClean;
        private float _targetClean;

        /// <summary>Builds the (inactive) overlay covering the body, as a child of <paramref name="car"/>.</summary>
        public static CarMudOverlay Attach(GameObject car, CarId id, CarDefinition definition, Sprite sprite)
        {
            var go = new GameObject("Mud");
            go.transform.SetParent(car.transform, false);
            go.SetActive(false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = CarSorting.Mud;
            renderer.color = definition.MudColor;
            var mud = go.AddComponent<CarMudOverlay>();
            mud._id = id;
            mud._definition = definition;
            mud._renderer = renderer;
            return mud;
        }

        private void OnEnable()
        {
            _shownClean = 0f;
            _targetClean = 0f;
            Apply();
            if (!Services.TryGet<IEventBus>(out _bus)) return;
            Services.TryGet(out _modifiers);
            _bus.Subscribe<ModifierProgressChanged>(OnProgress);
            _bus.Subscribe<ModifierCleared>(OnCleared);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<ModifierProgressChanged>(OnProgress);
            _bus.Unsubscribe<ModifierCleared>(OnCleared);
            _bus = null;
        }

        private void OnProgress(ModifierProgressChanged e)
        {
            if (e.Car == _id && ModifierLooks.Is(_modifiers, e.Modifier, ModifierLook.MudOverlay)) _targetClean = Mathf.Clamp01(e.Progress01);
        }

        private void OnCleared(ModifierCleared e)
        {
            if (e.Car == _id && ModifierLooks.Is(_modifiers, e.Modifier, ModifierLook.MudOverlay)) gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Mathf.Approximately(_shownClean, _targetClean)) return;
            _shownClean = CarTimerFillMath.Smooth(_shownClean, _targetClean, _definition.FillSmoothing, Time.deltaTime);
            Apply();
        }

        private void Apply()
        {
            var t = transform;
            t.localScale = new Vector3(CarMudMath.Width(_shownClean), 1f, 1f);
            t.localPosition = new Vector3(CarMudMath.CenterX(_shownClean), 0f, 0f);
        }
    }
}
