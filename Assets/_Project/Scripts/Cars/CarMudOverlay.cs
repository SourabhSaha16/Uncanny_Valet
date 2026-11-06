using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Semi-transparent brown overlay on a dirty car. Created inactive; CarTraitLook activates it. Its alpha
    /// fades as the car is washed (CarCleanChanged) and it disappears on CarCleaned.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarMudOverlay : MonoBehaviour
    {
        private CarId _id;
        private CarDefinition _definition;
        private SpriteRenderer _renderer;
        private IEventBus _bus;
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
            _bus.Subscribe<CarCleanChanged>(OnCleanChanged);
            _bus.Subscribe<CarCleaned>(OnCleaned);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<CarCleanChanged>(OnCleanChanged);
            _bus.Unsubscribe<CarCleaned>(OnCleaned);
            _bus = null;
        }

        private void OnCleanChanged(CarCleanChanged e)
        {
            if (e.Car == _id) _targetClean = Mathf.Clamp01(e.Clean01);
        }

        private void OnCleaned(CarCleaned e)
        {
            if (e.Car == _id) gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Mathf.Approximately(_shownClean, _targetClean)) return;
            _shownClean = CarTimerFillMath.Smooth(_shownClean, _targetClean, _definition.FillSmoothing, Time.deltaTime);
            Apply();
        }

        private void Apply()
        {
            Color mud = _definition.MudColor;
            mud.a = CarMudMath.Alpha(_shownClean, _definition.MudColor.a);
            _renderer.color = mud;
        }
    }
}
