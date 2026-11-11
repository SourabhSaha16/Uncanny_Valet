using System;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Makes one pedestrian stumble when the boosted valet barges them: listens for ValetBargedPedestrian addressed to this
    /// person's id and runs a <see cref="PedestrianStumbleRule"/>. The length comes from the F2 value, falling back to the config.
    /// </summary>
    public sealed class WalkerStumbler
    {
        private readonly PedestrianStumbleRule _rule = new PedestrianStumbleRule();
        private readonly Action<ValetBargedPedestrian> _handler;
        private readonly IEventBus _bus;
        private readonly ITuning _tuning;
        private readonly PedestrianConfig _config;
        private readonly WalkerSprites _sprites;
        private readonly int _id;
        private bool _subscribed;

        public WalkerStumbler(IEventBus bus, ITuning tuning, PedestrianConfig config, WalkerSprites sprites, int id)
        {
            _bus = bus;
            _tuning = tuning;
            _config = config;
            _sprites = sprites;
            _id = id;
            _rule.Configure(config.StumbleDistance, config.StumbleSpins);
            _handler = OnBarged;
        }

        /// <summary>Builds a stumbler using the registered event bus and tuning store (either may be missing; then it never starts).</summary>
        public static WalkerStumbler For(PedestrianConfig config, WalkerSprites sprites, int id)
        {
            Services.TryGet(out IEventBus bus);
            Services.TryGet(out ITuning tuning);
            return new WalkerStumbler(bus, tuning, config, sprites, id);
        }

        /// <summary>Raised when a stumble begins.</summary>
        public event Action Started;

        /// <summary>True while stumbling.</summary>
        public bool Active => _rule.Active;

        /// <summary>Current push away from the route point.</summary>
        public Vector2 Offset => _rule.Offset;

        /// <summary>Advances the stumble by one physics step and spins the sprites to match.</summary>
        public void Step(float deltaSeconds)
        {
            _rule.Step(deltaSeconds);
            _sprites.Spin(_rule.SpinDegrees);
        }

        /// <summary>Starts listening (no bus, no listening).</summary>
        public void Subscribe()
        {
            if (_subscribed || _bus == null) return;
            _subscribed = true;
            _bus.Subscribe(_handler);
        }

        /// <summary>Stops listening.</summary>
        public void Unsubscribe()
        {
            if (!_subscribed) return;
            _subscribed = false;
            _bus.Unsubscribe(_handler);
        }

        private void OnBarged(ValetBargedPedestrian e)
        {
            if (e.Pedestrian.Value != _id) return;
            var seconds = _tuning != null && _tuning.TryGet(TuningKey.PowerStumbleSeconds, out var tuned) ? tuned : _config.StumbleSeconds;
            if (_rule.TryStart(new Vector2(e.PushX, e.PushY), seconds)) Started?.Invoke();
        }
    }
}
