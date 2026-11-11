using System;
using UncannyValet.Core.Events;

namespace UncannyValet.Powers
{
    /// <summary>Owns the running powers. Each effect runs on its own timer, so a power keeps going while a higher one fires.</summary>
    public sealed class PowerStack
    {
        private readonly IEventBus _bus;
        private readonly IPowerEffect[] _effects;

        public PowerStack(IEventBus bus, IPowerEffect[] effects)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }

        public bool IsActive(PowerKind kind)
        {
            var effect = Find(kind);
            return effect != null && effect.IsActive;
        }

        /// <summary>Starts a power (or refreshes it if it is already running) and publishes PowerActivated. False if no effect handles the kind.</summary>
        public bool Fire(PowerKind kind)
        {
            var effect = Find(kind);
            if (effect == null) return false;
            effect.Activate();
            _bus.Publish(new PowerActivated(kind));
            return true;
        }

        /// <summary>Advances every effect; publishes PowerEnded for each timed power that ran out.</summary>
        public void Tick(float deltaSeconds)
        {
            for (var i = 0; i < _effects.Length; i++)
            {
                var wasActive = _effects[i].IsActive;
                _effects[i].Tick(deltaSeconds);
                if (wasActive && !_effects[i].IsActive) _bus.Publish(new PowerEnded(_effects[i].Kind));
            }
        }

        /// <summary>Stops every running power (a new shift starts clean), publishing PowerEnded for each.</summary>
        public void EndAll()
        {
            for (var i = 0; i < _effects.Length; i++)
            {
                if (!_effects[i].IsActive) continue;
                _effects[i].End();
                _bus.Publish(new PowerEnded(_effects[i].Kind));
            }
        }

        private IPowerEffect Find(PowerKind kind)
        {
            for (var i = 0; i < _effects.Length; i++)
                if (_effects[i].Kind == kind) return _effects[i];
            return null;
        }
    }
}
