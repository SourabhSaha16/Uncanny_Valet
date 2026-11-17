using System;
using UncannyValet.Core.Ids;

namespace UncannyValet.UI.Hud
{
    /// <summary>The one-time flash of each power icon, fading over a set time. Pure; the tracker decides when one starts.</summary>
    public sealed class PowerIconFlashes
    {
        private const int Slots = 8;

        private readonly float _seconds;
        private readonly float[] _left = new float[Slots];

        public PowerIconFlashes(float seconds) { _seconds = seconds; }

        /// <summary>1 right after the flash starts, fading to 0.</summary>
        public float Flash01(StoryPower power)
        {
            var slot = (int)power;
            return _seconds > 0f && slot >= 0 && slot < Slots ? _left[slot] / _seconds : 0f;
        }

        /// <summary>Starts the flash of a power.</summary>
        public void Begin(StoryPower power)
        {
            var slot = (int)power;
            if (slot >= 0 && slot < Slots) _left[slot] = _seconds;
        }

        /// <summary>Advances the fade by real seconds.</summary>
        public void Tick(float dt)
        {
            for (var i = 0; i < Slots; i++) _left[i] = Math.Max(0f, _left[i] - dt);
        }

        /// <summary>Stops every flash.</summary>
        public void Clear() => Array.Clear(_left, 0, Slots);
    }
}
