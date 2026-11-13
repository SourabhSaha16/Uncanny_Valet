using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Powers
{
    /// <summary>
    /// Fires an owned power on its number key (keys fill in the player's earn order) by spending one bucket of the power
    /// bar, and runs the powers' timers and cooldowns in real time. Registers IWorldTime and IPowerView.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PowerActivationController : MonoBehaviour
    {
        [SerializeField] private PowerBarController _bar;

        private IEventBus _bus;
        private IPowerInput _input;
        private PowerStack _stack;
        private DrivingTracker _driving;
        private bool _started;
        private bool _subscribed;

        /// <summary>The running powers and their cooldowns; null until Start.</summary>
        public PowerStack Stack => _stack;

        private void Start()
        {
            if (_bar == null)
            {
                Debug.LogError("PowerActivationController on " + name + " has no PowerBarController assigned.", this);
                enabled = false;
                return;
            }
            _bus = Services.Get<IEventBus>();
            _input = Services.TryGet<IPowerInput>(out var input) ? input : new KeyboardPowerInput();
            var freeze = new FreezeTimeEffect(_bar.Settings);
            _driving = new DrivingTracker(_bus);
            var thunder = new ThunderStridePower(() => _driving.IsDriving);
            _stack = new PowerStack(_bus, new IPower[] { thunder, freeze }, new PowerCooldowns());
            Services.Register<IWorldTime>(freeze);
            Services.Register<IPowerView>(new PowerView(() => _bar.Progress, _bar.Settings, _stack));
            _started = true;
            Subscribe();
        }

        private void OnEnable() { if (_started) Subscribe(); }

        private void OnDisable() => Unsubscribe();

        private void OnDestroy() => _driving?.Dispose();

        private void Update()
        {
            if (_stack == null) return;
            for (var slot = 0; slot < PowerKeyMap.SlotCount; slot++)
                if (_input.PowerPressed(slot)) TryFire(slot);
            _stack.Tick(Time.unscaledDeltaTime);
        }

        private void TryFire(int slot)
        {
            if (!_bar.IsActive || !PowerKeyMap.TryGetPower(_bar.Progress.EarnedPowers, slot, out var power)) return;
            var level = _bar.Progress.PowerLevel(power);
            if (_bar.Settings.TryGetValues(power, level, out var values)) _stack.TryFire(power, level, values, _bar);
        }

        private void OnShiftStarted(ShiftStarted e) => _stack.EndAll();

        private void Subscribe()
        {
            if (_subscribed) return;
            _subscribed = true;
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            _subscribed = false;
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
        }
    }
}
