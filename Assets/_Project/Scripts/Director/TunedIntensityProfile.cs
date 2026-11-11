using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>
    /// The lot profile as the difficulty dials reshape it: the target curve is flattened or exaggerated around its
    /// average, scaled, and boosted near the end; the base archetype weights come from tuning. With neutral
    /// <see cref="ShapeSettings"/> every answer is the lot profile's own, bit for bit.
    /// </summary>
    public sealed class TunedIntensityProfile : IIntensityProfile
    {
        private const int AreaSteps = 32;

        private readonly IIntensityProfile _base;
        private readonly ShapeSettings _shape;
        private readonly float _lookahead;
        private readonly float _average;

        /// <param name="lotProfile">The lot's own profile.</param>
        /// <param name="shape">Live shape numbers (refreshed by the director every tick).</param>
        /// <param name="risingLookahead">Fraction of the shift looked ahead to decide whether the curve is rising.</param>
        public TunedIntensityProfile(IIntensityProfile lotProfile, ShapeSettings shape, float risingLookahead)
        {
            _base = lotProfile; _shape = shape; _lookahead = risingLookahead;
            _average = lotProfile.AreaFrom(0f, 1f);
        }

        public float Sample(float shiftFraction)
        {
            if (_shape.IsNeutral) return _base.Sample(shiftFraction);
            var fraction = Mathf.Clamp01(shiftFraction);
            var shaped = _base.Sample(fraction) * _shape.RampShape + _average * (1f - _shape.RampShape);
            var span = Mathf.Max(Mathf.Epsilon, 1f - _shape.LateStart);
            var late = Mathf.Clamp01((fraction - _shape.LateStart) / span);
            return Mathf.Max(0f, shaped * _shape.TargetScale * (1f + _shape.LateBoost * late));
        }

        public DirectorPhase PhaseAt(float shiftFraction) => _base.PhaseAt(shiftFraction);

        public float AreaFrom(float shiftFraction, float shiftLengthSeconds)
        {
            if (_shape.IsNeutral) return _base.AreaFrom(shiftFraction, shiftLengthSeconds);
            var from = Mathf.Clamp01(shiftFraction);
            var step = (1f - from) / AreaSteps;
            var sum = 0f;
            for (var i = 0; i < AreaSteps; i++) sum += Sample(from + (i + 0.5f) * step);
            return sum * step * shiftLengthSeconds;
        }

        public bool IsRising(float shiftFraction)
            => _shape.IsNeutral ? _base.IsRising(shiftFraction) : Sample(shiftFraction + _lookahead) > Sample(shiftFraction);

        public ArchetypeWeights Weights => _shape.HasWeights ? _shape.Weights : _base.Weights;
    }
}
