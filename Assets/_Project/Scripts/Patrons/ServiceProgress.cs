using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>A 0..1 meter that fills by a fixed step per game minute of service.</summary>
    public sealed class ServiceProgress
    {
        private readonly float _stepPerMinute;

        public ServiceProgress(float start, float minutesToFull)
        {
            Value = Mathf.Clamp01(start);
            _stepPerMinute = minutesToFull <= 0f ? 1f : 1f / minutesToFull;
        }

        public float Value { get; private set; }
        public bool IsFull => Value >= 1f;

        /// <summary>Fills the meter at once. Returns true if the value changed.</summary>
        public bool Complete()
        {
            if (IsFull) return false;
            Value = 1f;
            return true;
        }

        /// <summary>Adds one minute of service. Returns true if the value changed.</summary>
        public bool Step()
        {
            if (IsFull) return false;
            Value = Mathf.Min(1f, Value + _stepPerMinute);
            return true;
        }
    }
}
