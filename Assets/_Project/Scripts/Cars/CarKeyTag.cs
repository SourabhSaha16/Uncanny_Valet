using UncannyValet.Core;
using UncannyValet.Core.Config;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// The ticket's key tag on a car: a small coloured stripe on the roof. Once the patron wants the car back, the
    /// stripe and a ring around the car flash; the flash speeds up as the pickup patience runs out and turns amber,
    /// then orange-red. The flash runs on real time (not game time) and allocates nothing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarKeyTag : MonoBehaviour
    {
        private CarId _id;
        private CarFlashConfig _config;
        private SpriteRenderer _stripe;
        private SpriteRenderer _outline;
        private IEventBus _bus;
        private Color _tagColor = TagPalette.Unknown;
        private bool _flashing;
        private bool _expired;
        private float _elapsed01;
        private float _phase;

        /// <summary>Adds the stripe, its dark border and the (hidden) outline to <paramref name="car"/>.</summary>
        public static CarKeyTag Attach(GameObject car, CarId id, Vector2 carSize, Sprite sprite, CarFlashConfig config)
        {
            var tag = car.AddComponent<CarKeyTag>();
            tag._id = id;
            tag._config = config;
            var border = config.TagBorder * 2f;
            Child(car, "KeyTagBacking", sprite, CarSorting.TagBacking, config.TagBackingColor,
                (config.TagSize.x + border) / carSize.x, (config.TagSize.y + border) / carSize.y);
            tag._stripe = Child(car, "KeyTag", sprite, CarSorting.Tag, TagPalette.Unknown,
                config.TagSize.x / carSize.x, config.TagSize.y / carSize.y);
            var pad = config.OutlinePadding * 2f;
            tag._outline = Child(car, "PickupOutline", sprite, CarSorting.Outline, Color.white,
                (carSize.x + pad) / carSize.x, (carSize.y + pad) / carSize.y);
            tag._outline.enabled = false;
            return tag;
        }

        private static SpriteRenderer Child(GameObject car, string name, Sprite sprite, int order, Color color,
            float scaleX, float scaleY)
        {
            var go = new GameObject(name);
            go.transform.SetParent(car.transform, false);
            go.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        private void OnEnable()
        {
            if (!Services.TryGet<IEventBus>(out _bus)) return;
            _bus.Subscribe<CarTagAssigned>(OnTag);
            _bus.Subscribe<PickupRequested>(OnPickup);
            _bus.Subscribe<TicketTimerChanged>(OnChanged);
            _bus.Subscribe<TicketTimerExpired>(OnExpired);
            _bus.Subscribe<TicketTimerStopped>(OnStopped);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<CarTagAssigned>(OnTag);
            _bus.Unsubscribe<PickupRequested>(OnPickup);
            _bus.Unsubscribe<TicketTimerChanged>(OnChanged);
            _bus.Unsubscribe<TicketTimerExpired>(OnExpired);
            _bus.Unsubscribe<TicketTimerStopped>(OnStopped);
            _bus = null;
        }

        private void OnTag(CarTagAssigned e)
        {
            if (e.Car != _id) return;
            _tagColor = TagPalette.Get(e.Tag);
            _stripe.color = _tagColor;
        }

        private void OnPickup(PickupRequested e)
        {
            if (e.Car != _id) return;
            _flashing = true;
            _expired = false;
            _elapsed01 = 0f;
            _phase = 0f;
        }

        private void OnChanged(TicketTimerChanged e)
        {
            if (e.Car == _id && e.Kind == TimerKind.Pickup) _elapsed01 = e.Elapsed01;
        }

        private void OnExpired(TicketTimerExpired e)
        {
            if (e.Car == _id && e.Kind == TimerKind.Pickup) _expired = true;
        }

        private void OnStopped(TicketTimerStopped e)
        {
            if (e.Car != _id || !_flashing) return;
            _flashing = false;
            _outline.enabled = false;
            _stripe.color = _tagColor;
        }

        private void Update()
        {
            if (!_flashing) return;
            var level = PickupFlashMath.Level(_elapsed01, _expired, _config.AmberFrom, _config.DangerFrom);
            var hz = PickupFlashMath.Frequency(_expired ? 1f : _elapsed01, _config.MinHz, _config.MaxHz);
            _phase = PickupFlashMath.Advance(_phase, hz, Time.unscaledDeltaTime);
            var on = PickupFlashMath.IsOn(_phase);
            var color = _config.ColorFor(level);
            _outline.color = color;
            _outline.enabled = on;
            _stripe.color = on ? color : _tagColor;
        }
    }
}
