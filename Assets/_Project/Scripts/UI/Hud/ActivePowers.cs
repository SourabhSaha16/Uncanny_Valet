using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure countdown of the powers on screen, indexed by meter level (1 speed boost, 2 freeze time, 3 park smash).
    /// A start sets the timer, a refresh restarts it, an end or the timer reaching zero clears it.
    /// </summary>
    public sealed class ActivePowers
    {
        public const int Count = 3;
        private readonly float[] _duration = new float[Count];
        private readonly float[] _remaining = new float[Count];

        public ActivePowers(PowerMeterSettings settings)
        {
            for (var i = 0; i < Count; i++) _duration[i] = settings.DurationOf(i + 1);
        }

        /// <summary>True while the power (1 to 3) is showing.</summary>
        public bool IsActive(int power) => Valid(power) && _remaining[power - 1] > 0f;

        /// <summary>Fraction of the power's time left, 1 down to 0.</summary>
        public float Remaining01(int power)
            => Valid(power) && _duration[power - 1] > 0f ? _remaining[power - 1] / _duration[power - 1] : 0f;

        public void Start(int power)
        {
            if (Valid(power)) _remaining[power - 1] = _duration[power - 1];
        }

        public void End(int power)
        {
            if (Valid(power)) _remaining[power - 1] = 0f;
        }

        public void Clear() => Array.Clear(_remaining, 0, Count);

        public void Tick(float dt)
        {
            for (var i = 0; i < Count; i++) _remaining[i] = Math.Max(0f, _remaining[i] - dt);
        }

        private static bool Valid(int power) => power >= 1 && power <= Count;
    }
}
