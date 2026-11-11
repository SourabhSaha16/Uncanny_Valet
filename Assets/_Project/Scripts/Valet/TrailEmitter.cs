using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>Turns a rate (sparkles per second) into whole sparkles per frame, carrying the fraction over so the rate is exact over time.</summary>
    public sealed class TrailEmitter
    {
        private float _carry;

        /// <summary>How many sparkles to emit after <paramref name="deltaSeconds"/> at <paramref name="perSecond"/>.</summary>
        public int Step(float deltaSeconds, float perSecond)
        {
            _carry += Mathf.Max(0f, perSecond) * Mathf.Max(0f, deltaSeconds);
            var whole = Mathf.FloorToInt(_carry);
            _carry -= whole;
            return whole;
        }

        /// <summary>Drops the fraction left over (the trail stopped).</summary>
        public void Reset() => _carry = 0f;
    }
}
