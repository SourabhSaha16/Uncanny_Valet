using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Powers
{
    /// <summary>Fires the highest unlocked power on the activate button and runs the active powers' timers. Registers IWorldTime.</summary>
    [DisallowMultipleComponent]
    public sealed class PowerActivationController : MonoBehaviour
    {
        [SerializeField] private PowerMeterController _meter;

        private IEventBus _bus;
        private IPowerInput _input;
        private PowerStack _stack;
        private bool _started;
        private bool _subscribed;

        /// <summary>The speed boost state, for the Valet module to read its walk multiplier (null before Start).</summary>
        public SpeedBoostEffect SpeedBoost { get; private set; }

        private void Start()
        {
            if (_meter == null)
            {
                Debug.LogError("PowerActivationController on " + name + " has no PowerMeterController assigned.", this);
                enabled = false;
                return;
            }
            _bus = Services.Get<IEventBus>();
            _input = Services.TryGet<IPowerInput>(out var input) ? input : new KeyboardPowerInput();
            SpeedBoost = new SpeedBoostEffect(_meter.Settings);
            var freeze = new FreezeTimeEffect(_meter.Settings);
            _stack = new PowerStack(_bus, new IPowerEffect[] { SpeedBoost, freeze, new ParkSmashEffect() });
            Services.Register<IWorldTime>(freeze);
            Services.Register<IPowerBoost>(SpeedBoost);
            _started = true;
            Subscribe();
        }

        private void OnEnable() { if (_started) Subscribe(); }

        private void OnDisable() => Unsubscribe();

        private void Update()
        {
            if (_stack == null) return;
            if (_input.ActivatePressed && _meter.TrySpend(out var level)) _stack.Fire((PowerKind)level);
            _stack.Tick(Time.deltaTime);
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
