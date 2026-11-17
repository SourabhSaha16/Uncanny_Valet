using System;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// The glow of Thunder stride, for the whole boost: a pulsing golden tint on the valet (or the car he drives), a soft glow
    /// under it, and a steady spray of lightning sparks. It blinks in the last seconds, and clears (tint, glow) the moment the
    /// boost ends. Follows the target, so getting in or out of a car moves the effect over. Nothing is allocated per frame.
    /// </summary>
    public sealed class ThunderAura : IDisposable
    {
        private readonly ThunderAuraClock _clock;
        private readonly ThunderAuraLook _look;
        private readonly Action<Vector2, Vector2> _spark;
        private readonly AuraTint _tint = new AuraTint();
        private readonly AuraGlow _glow = new AuraGlow();
        private Transform _bound;
        private bool _hasTarget;
        private float _sparkIn;

        /// <param name="spark">Draws one lightning streak between two world points.</param>
        public ThunderAura(ThunderAuraTiming timing, ThunderAuraLook look, Action<Vector2, Vector2> spark)
        {
            _clock = new ThunderAuraClock(timing);
            _look = look;
            _spark = spark ?? throw new ArgumentNullException(nameof(spark));
        }

        /// <summary>True while the glow is showing.</summary>
        public bool IsActive => _clock.IsActive;

        /// <summary>Starts the glow for the boost length; starting it again restarts it.</summary>
        public void Start(float seconds)
        {
            _clock.Start(seconds);
            _sparkIn = 0f;
        }

        /// <summary>Ends the glow now and takes the tint and glow off.</summary>
        public void Clear()
        {
            _clock.Clear();
            Release();
        }

        /// <summary>Advances the glow by real seconds and draws it on <paramref name="target"/> (the valet, or the car he drives).</summary>
        public void Tick(float deltaSeconds, Transform target)
        {
            _clock.Tick(deltaSeconds);
            if (!_clock.IsActive || target == null) { Release(); return; }
            if (target != _bound)
            {
                _tint.Bind(target);
                _glow.Bind(_tint.Renderers, _look.GlowScale);
                _bound = target;
                _hasTarget = true;
            }
            var strength = _clock.Strength;
            _tint.Apply(Color.Lerp(Color.white, _look.Tint, strength));
            _glow.Show(target.position, _look.Glow, strength);
            _sparkIn -= deltaSeconds;
            if (_sparkIn > 0f) return;
            _sparkIn = _look.SparkInterval;
            var at = (Vector2)target.position;
            var reach = _look.SparkReach;
            _spark(at + new Vector2(UnityEngine.Random.Range(-reach, reach), reach), at + new Vector2(UnityEngine.Random.Range(-reach, reach), -reach));
        }

        /// <summary>Clears the glow and destroys what it made.</summary>
        public void Dispose()
        {
            Clear();
            _glow.Dispose();
        }

        private void Release()
        {
            if (!_hasTarget) return;
            _tint.Bind(null);
            _glow.Hide();
            _bound = null;
            _hasTarget = false;
        }
    }
}
