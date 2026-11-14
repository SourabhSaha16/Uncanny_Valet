using System;

namespace UncannyValet.Audio
{
    /// <summary>
    /// The last-30-seconds escalation of a themed lot: the tempo eases from 1 to 1 + the rise (0.15 = 15% faster) and the extra layer
    /// fades in over its ramp time. Replaces the urgency rule in themed lots.
    /// </summary>
    public sealed class EscalationRule
    {
        private readonly float _tempoRise;
        private readonly float _smoothingSeconds;
        private readonly float _rampSeconds;
        private float _elapsed;

        public EscalationRule(float tempoRise, float smoothingSeconds, float rampSeconds)
        {
            _tempoRise = Math.Max(0f, tempoRise);
            _smoothingSeconds = Math.Max(0f, smoothingSeconds);
            _rampSeconds = Math.Max(0f, rampSeconds);
            Tempo = 1f;
        }

        /// <summary>True from the Last30Seconds event until the next reset.</summary>
        public bool Active { get; private set; }

        /// <summary>The smoothed tempo multiplier right now.</summary>
        public float Tempo { get; private set; }

        /// <summary>The tempo multiplier the rule is heading for.</summary>
        public float TargetTempo => Active ? 1f + _tempoRise : 1f;

        /// <summary>0 to 1: how far the extra layer has faded in.</summary>
        public float LayerLevel => !Active ? 0f : _rampSeconds <= 0f ? 1f : Math.Min(1f, _elapsed / _rampSeconds);

        /// <summary>The last 30 seconds began.</summary>
        public void Trigger()
        {
            Active = true;
            _elapsed = 0f;
        }

        /// <summary>Back to normal tempo and no extra layer (a new shift, or the shift ended).</summary>
        public void Reset()
        {
            Active = false;
            Tempo = 1f;
            _elapsed = 0f;
        }

        /// <summary>Advances the time and returns the tempo multiplier. Allocation-free.</summary>
        public float Step(float deltaSeconds)
        {
            if (Active) _elapsed += Math.Max(0f, deltaSeconds);
            if (_smoothingSeconds <= 0f) Tempo = TargetTempo;
            else if (deltaSeconds > 0f) Tempo += (TargetTempo - Tempo) * (1f - (float)Math.Exp(-deltaSeconds / _smoothingSeconds));
            return Tempo;
        }
    }
}
