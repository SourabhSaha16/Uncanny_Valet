using UncannyValet.Core.Events;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>
    /// A plain-C# intensity profile for the soak test: the Diner's default curve as straight segments (no
    /// AnimationCurve, so the soak runs without the Unity engine), Diner weights and the same phases.
    /// </summary>
    internal sealed class SoakProfile : IIntensityProfile
    {
        private const float Lookahead = 0.03f;
        private const int AreaSteps = 32;
        private static readonly float[] Xs = { 0f, 0.15f, 0.30f, 0.42f, 0.60f, 0.72f, 0.85f, 1f };
        private static readonly float[] Ys = { 0.35f, 0.7f, 0.95f, 0.5f, 1.05f, 0.55f, 1.1f, 1.15f };
        private static readonly float[] PhaseStarts = { 0f, 0.22f, 0.36f, 0.48f, 0.68f, 0.78f };
        private static readonly DirectorPhase[] Phases =
        {
            DirectorPhase.BuildUp, DirectorPhase.Peak, DirectorPhase.Breather, DirectorPhase.HarderPeak,
            DirectorPhase.Breather, DirectorPhase.Finale,
        };

        public ArchetypeWeights Weights => ArchetypeWeights.Diner;

        public float Sample(float shiftFraction)
        {
            var f = shiftFraction < 0f ? 0f : shiftFraction > 1f ? 1f : shiftFraction;
            for (var i = 1; i < Xs.Length; i++)
            {
                if (f > Xs[i]) continue;
                var t = (f - Xs[i - 1]) / (Xs[i] - Xs[i - 1]);
                return Ys[i - 1] + (Ys[i] - Ys[i - 1]) * t;
            }
            return Ys[Ys.Length - 1];
        }

        public DirectorPhase PhaseAt(float shiftFraction)
        {
            var phase = Phases[0];
            for (var i = 0; i < PhaseStarts.Length; i++) if (PhaseStarts[i] <= shiftFraction) phase = Phases[i];
            return phase;
        }

        public float AreaFrom(float shiftFraction, float shiftLengthSeconds)
        {
            var from = shiftFraction < 0f ? 0f : shiftFraction > 1f ? 1f : shiftFraction;
            var step = (1f - from) / AreaSteps;
            var sum = 0f;
            for (var i = 0; i < AreaSteps; i++) sum += Sample(from + (i + 0.5f) * step);
            return sum * step * shiftLengthSeconds;
        }

        public bool IsRising(float shiftFraction) => Sample(shiftFraction + Lookahead) > Sample(shiftFraction);
    }
}
