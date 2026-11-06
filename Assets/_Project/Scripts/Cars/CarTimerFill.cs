using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Dark overlay that rises over the car as its ticket timer runs out (design.md section 10).
    /// Events arrive about once a second, so the shown fill eases toward the latest target.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarTimerFill : MonoBehaviour
    {
        private CarId _id;
        private CarDefinition _definition;
        private SpriteRenderer _overlay;
        private Transform _overlayTransform;
        private IEventBus _bus;
        private float _shown;
        private float _target;
        private bool _active;

        /// <summary>Creates the overlay child on <paramref name="car"/> using a shared 1x1 sprite.</summary>
        public static CarTimerFill Attach(GameObject car, CarId id, CarDefinition definition, Sprite sprite)
        {
            var go = new GameObject("TimerFill");
            go.transform.SetParent(car.transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = CarSorting.TimerFill;
            renderer.enabled = false;
            var fill = car.AddComponent<CarTimerFill>();
            fill._id = id;
            fill._definition = definition;
            fill._overlay = renderer;
            fill._overlayTransform = go.transform;
            return fill;
        }

        private void OnEnable()
        {
            if (!Services.TryGet<IEventBus>(out _bus)) return;
            _bus.Subscribe<TicketTimerChanged>(OnChanged);
            _bus.Subscribe<TicketTimerExpired>(OnExpired);
            _bus.Subscribe<TicketTimerStopped>(OnStopped);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<TicketTimerChanged>(OnChanged);
            _bus.Unsubscribe<TicketTimerExpired>(OnExpired);
            _bus.Unsubscribe<TicketTimerStopped>(OnStopped);
            _bus = null;
        }

        private void OnChanged(TicketTimerChanged e)
        {
            if (e.Car != _id) return;
            if (!_active) _shown = 0f;
            _active = true;
            _target = Mathf.Clamp01(e.Elapsed01);
            _overlay.color = e.Kind == TimerKind.Pickup ? _definition.PickupFillColor : _definition.ParkFillColor;
            _overlay.enabled = true;
        }

        private void OnExpired(TicketTimerExpired e)
        {
            if (e.Car == _id && _active) _target = 1f;
        }

        private void OnStopped(TicketTimerStopped e)
        {
            if (e.Car != _id) return;
            _active = false;
            _shown = 0f;
            _overlay.enabled = false;
        }

        private void Update()
        {
            if (!_active) return;
            _shown = CarTimerFillMath.Smooth(_shown, _target, _definition.FillSmoothing, Time.deltaTime);
            _overlayTransform.localScale = new Vector3(1f, CarTimerFillMath.HeightScale(_shown), 1f);
            _overlayTransform.localPosition = new Vector3(0f, CarTimerFillMath.CentreY(_shown), 0f);
        }
    }
}
