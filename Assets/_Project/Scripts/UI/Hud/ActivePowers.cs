using UncannyValet.Core.Events;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure countdown of the timed power on screen (freeze time). A start sets the timer, a refresh restarts it,
    /// an end or the timer reaching zero clears it. Other power kinds are ignored.
    /// </summary>
    public sealed class ActivePowers
    {
        private readonly float _freezeDuration;
        private float _freezeRemaining;

        public ActivePowers(PowerBarSettings settings) { _freezeDuration = settings.FreezeSeconds; }

        /// <summary>True while the power is showing.</summary>
        public bool IsActive(PowerKind power) => power == PowerKind.FreezeTime && _freezeRemaining > 0f;

        /// <summary>Fraction of the power's time left, 1 down to 0.</summary>
        public float Remaining01(PowerKind power)
            => power == PowerKind.FreezeTime && _freezeDuration > 0f ? _freezeRemaining / _freezeDuration : 0f;

        public void Start(PowerKind power)
        {
            if (power == PowerKind.FreezeTime) _freezeRemaining = _freezeDuration;
        }

        public void End(PowerKind power)
        {
            if (power == PowerKind.FreezeTime) _freezeRemaining = 0f;
        }

        public void Clear() => _freezeRemaining = 0f;

        public void Tick(float dt) => _freezeRemaining = System.Math.Max(0f, _freezeRemaining - dt);
    }
}
