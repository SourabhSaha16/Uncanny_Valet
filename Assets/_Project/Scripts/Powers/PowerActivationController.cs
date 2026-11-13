using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Powers
{
    /// <summary>
    /// Fires an owned power on its number key by spending one bucket of the power bar, and runs the active powers' timers.
    /// Registers IWorldTime. Key 1 is the first power earned (until the power framework adds the other keys).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PowerActivationController : MonoBehaviour
    {
        [SerializeField] private PowerBarController _bar;

        private IEventBus _bus;
        private IPowerInput _input;
        private PowerStack _stack;
        private bool _started;
        private bool _subscribed;

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
            _stack = new PowerStack(_bus, new IPowerEffect[] { freeze });
            Services.Register<IWorldTime>(freeze);
            _started = true;
            Subscribe();
        }

        private void OnEnable() { if (_started) Subscribe(); }

        private void OnDisable() => Unsubscribe();

        private void Update()
        {
            if (_stack == null) return;
            for (var slot = 0; slot < PowerKeyMap.SlotCount; slot++)
                if (_input.PowerPressed(slot)) TryFire(slot);
            _stack.Tick(Time.deltaTime);
        }

        private void TryFire(int slot)
        {
            if (!_bar.IsActive || !PowerKeyMap.TryGetKind(_bar.Progress.EarnedPowers, slot, out var kind)) return;
            if (_bar.TrySpend()) _stack.Fire(kind);
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
