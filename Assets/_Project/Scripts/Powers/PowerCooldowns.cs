using UncannyValet.Core.Ids;

namespace UncannyValet.Powers
{
    /// <summary>
    /// Cooldown clocks of the story powers, counted in the real seconds the caller passes (never scaled by freeze time).
    /// A cooldown starts only when the caller starts it (after the power's use ends) and is ready again at every lot start.
    /// </summary>
    public sealed class PowerCooldowns
    {
        private const int Slots = 8;

        private readonly float[] _remaining = new float[Slots];
        private readonly float[] _total = new float[Slots];

        /// <summary>True when the power is not cooling down.</summary>
        public bool IsReady(StoryPower power) => _remaining[Slot(power)] <= 0f;

        /// <summary>Seconds of cooldown left.</summary>
        public float Remaining(StoryPower power) => _remaining[Slot(power)];

        /// <summary>Share of the cooldown left, 1 down to 0.</summary>
        public float Remaining01(StoryPower power)
        {
            var slot = Slot(power);
            return _total[slot] > 0f ? _remaining[slot] / _total[slot] : 0f;
        }

        /// <summary>Starts a cooldown of the given length (0 or less leaves the power ready).</summary>
        public void Start(StoryPower power, float seconds)
        {
            var slot = Slot(power);
            _remaining[slot] = seconds > 0f ? seconds : 0f;
            _total[slot] = _remaining[slot];
        }

        /// <summary>Advances every cooldown.</summary>
        public void Tick(float deltaSeconds)
        {
            for (var i = 0; i < Slots; i++)
                if (_remaining[i] > 0f) _remaining[i] = _remaining[i] > deltaSeconds ? _remaining[i] - deltaSeconds : 0f;
        }

        /// <summary>Makes every power ready (a new lot starts clean).</summary>
        public void ResetAll()
        {
            for (var i = 0; i < Slots; i++)
            {
                _remaining[i] = 0f;
                _total[i] = 0f;
            }
        }

        private static int Slot(StoryPower power)
        {
            var slot = (int)power;
            return slot >= 0 && slot < Slots ? slot : 0;
        }
    }
}
