using UncannyValet.Core;
using UncannyValet.Core.Config;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// The ticket's colour on the car: the body sprite is tinted to the tag colour (softened toward white). Once the
    /// patron wants the car back, the tint pulses toward the alarm colour; the pulse speeds up as the pickup patience
    /// runs out and turns amber, then orange-red. The pulse runs on real time (not game time) and allocates nothing.
    /// A car without sprite art (coloured rectangle) is not tinted.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarKeyTag : MonoBehaviour
    {
        private CarId _id;
        private CarFlashConfig _config;
        private CarBodyArt _art;
        private IEventBus _bus;
        private IModifierCatalog _modifiers;
        private Color _tagColor = Color.white;
        private Color _tint = Color.white;
        private bool _isEv;
        private bool _flashing;
        private bool _expired;
        private float _elapsed01;
        private float _phase;

        /// <summary>Adds the tint behaviour to <paramref name="car"/>; <paramref name="art"/> may be null (no tint).</summary>
        public static CarKeyTag Attach(GameObject car, CarId id, CarBodyArt art, CarFlashConfig config)
        {
            var tag = car.AddComponent<CarKeyTag>();
            tag._id = id;
            tag._art = art;
            tag._config = config;
            return tag;
        }

        private void OnEnable()
        {
            if (!Services.TryGet<IEventBus>(out _bus)) return;
            Services.TryGet(out _modifiers);
            _bus.Subscribe<CarTagAssigned>(OnTag);
            _bus.Subscribe<TicketModifiersAssigned>(OnModifiers);
            _bus.Subscribe<PickupRequested>(OnPickup);
            _bus.Subscribe<TicketTimerChanged>(OnChanged);
            _bus.Subscribe<TicketTimerExpired>(OnExpired);
            _bus.Subscribe<TicketTimerStopped>(OnStopped);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<CarTagAssigned>(OnTag);
            _bus.Unsubscribe<TicketModifiersAssigned>(OnModifiers);
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
            ApplyTint();
        }

        private void OnModifiers(TicketModifiersAssigned e)
        {
            if (e.Car != _id) return;
            _isEv = ModifierLooks.Has(_modifiers, e.Modifiers, ModifierLook.ElectricBody);
            ApplyTint();
        }

        private void ApplyTint()
        {
            _tint = CarTintMath.BodyTint(_tagColor, _config.TintSoftness, _isEv, _config.EvTintStrength);
            if (_art != null) _art.SetTint(_tint);
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
            if (_art != null) _art.SetTint(_tint);
        }

        private void Update()
        {
            if (!_flashing || _art == null) return;
            var level = PickupFlashMath.Level(_elapsed01, _expired, _config.AmberFrom, _config.DangerFrom);
            var hz = PickupFlashMath.Frequency(_expired ? 1f : _elapsed01, _config.MinHz, _config.MaxHz);
            _phase = PickupFlashMath.Advance(_phase, hz, Time.unscaledDeltaTime);
            _art.SetTint(CarTintMath.Pulse(_tint, _config.ColorFor(level), _config.PulseMix, PickupFlashMath.IsOn(_phase)));
        }
    }
}
