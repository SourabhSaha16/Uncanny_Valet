using System;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Makes one pedestrian stumble: runs a <see cref="PedestrianStumbleRule"/> and spins the sprites to match. Whoever
    /// knocks the person over (the pedestrian knockback power) calls <see cref="TryStart"/>; the length comes from the config.
    /// </summary>
    public sealed class WalkerStumbler
    {
        private readonly PedestrianStumbleRule _rule = new PedestrianStumbleRule();
        private readonly PedestrianConfig _config;
        private readonly WalkerSprites _sprites;

        public WalkerStumbler(PedestrianConfig config, WalkerSprites sprites)
        {
            _config = config;
            _sprites = sprites;
            _rule.Configure(config.StumbleDistance, config.StumbleSpins);
        }

        /// <summary>Raised when a stumble begins.</summary>
        public event Action Started;

        /// <summary>True while stumbling.</summary>
        public bool Active => _rule.Active;

        /// <summary>Current push away from the route point.</summary>
        public Vector2 Offset => _rule.Offset;

        /// <summary>Starts a stumble away along <paramref name="push"/>. False while already stumbling or when the push is empty.</summary>
        public bool TryStart(Vector2 push)
        {
            if (!_rule.TryStart(push, _config.StumbleSeconds)) return false;
            Started?.Invoke();
            return true;
        }

        /// <summary>Starts a stumble that throws the person <paramref name="distance"/> units away along <paramref name="push"/> and lasts <paramref name="seconds"/>.</summary>
        public bool TryStart(Vector2 push, float distance, float seconds)
        {
            if (!_rule.TryStart(push, seconds, distance)) return false;
            Started?.Invoke();
            return true;
        }

        /// <summary>Advances the stumble by one physics step and spins the sprites to match.</summary>
        public void Step(float deltaSeconds)
        {
            _rule.Step(deltaSeconds);
            _sprites.Spin(_rule.SpinDegrees);
        }
    }
}
