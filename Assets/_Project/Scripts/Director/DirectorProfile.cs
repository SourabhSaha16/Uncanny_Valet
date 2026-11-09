using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>
    /// One lot's director settings: the target intensity curve T(t), the phases of the shift, the base archetype
    /// weights and the arrival budget. Read-only at runtime. Time on the curve is the fraction of the shift (0..1),
    /// so a longer or shorter shift keeps the same shape.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Director Profile", fileName = "DirectorProfile")]
    public sealed class DirectorProfile : ScriptableObject
    {
        /// <summary>Cars a shift is meant to bring (the Diner's base count, spec section 5).</summary>
        public const int DefaultArrivalBudget = 14;

        /// <summary>Extra cars allowed beyond the budget.</summary>
        public const int DefaultBudgetSlack = 2;

        private const int AreaSteps = 32;

        [SerializeField, Tooltip("Target workload T over the shift: x is the fraction of the shift (0..1), y is the workload the director aims for (1 = exactly busy).")]
        private AnimationCurve _intensity = DefaultCurve();
        [SerializeField, Tooltip("Phase names by start fraction, in order: build-up, peak, breather, harder peak, breather, finale.")]
        private PhaseSpan[] _phases = DefaultPhases();

        [Header("Base archetype weights (used by the archetype picker, later)")]
        [SerializeField, Min(0f)] private float _plainWeight = 50f;
        [SerializeField, Min(0f)] private float _dirtyWeight = 20f;
        [SerializeField, Min(0f)] private float _evWeight = 20f;
        [SerializeField, Min(0f)] private float _dirtyEvWeight = 10f;

        [Header("Arrival budget")]
        [SerializeField, Min(1), Tooltip("Cars the shift is meant to bring.")]
        private int _arrivalBudget = DefaultArrivalBudget;
        [SerializeField, Min(0), Tooltip("Extra cars allowed beyond the budget; budget + slack is a hard cap.")]
        private int _budgetSlack = DefaultBudgetSlack;

        public int ArrivalBudget => _arrivalBudget;
        public int BudgetSlack => _budgetSlack;

        /// <summary>The target workload T at a fraction (0..1) of the shift.</summary>
        public float Sample(float shiftFraction) => Mathf.Max(0f, _intensity.Evaluate(Mathf.Clamp01(shiftFraction)));

        /// <summary>The phase at a fraction of the shift: the last span that has started.</summary>
        public DirectorPhase PhaseAt(float shiftFraction)
        {
            var fraction = Mathf.Clamp01(shiftFraction);
            var phase = _phases.Length > 0 ? _phases[0].Phase : DirectorPhase.BuildUp;
            for (var i = 0; i < _phases.Length; i++)
                if (_phases[i].Start01 <= fraction) phase = _phases[i].Phase;
            return phase;
        }

        /// <summary>The area under T from a fraction of the shift to its end, in workload-seconds for a shift of this length.</summary>
        public float AreaFrom(float shiftFraction, float shiftLengthSeconds)
        {
            var from = Mathf.Clamp01(shiftFraction);
            var step = (1f - from) / AreaSteps;
            var sum = 0f;
            for (var i = 0; i < AreaSteps; i++) sum += Sample(from + (i + 0.5f) * step);
            return sum * step * shiftLengthSeconds;
        }

        /// <summary>Base weight of an archetype in the draw.</summary>
        public float WeightOf(CarArchetype archetype)
        {
            switch (archetype)
            {
                case CarArchetype.Dirty: return _dirtyWeight;
                case CarArchetype.Ev: return _evWeight;
                case CarArchetype.DirtyEv: return _dirtyEvWeight;
                default: return _plainWeight;
            }
        }

        // Build-up, peak, breather, harder peak, breather, finale (spec section 3).
        private static AnimationCurve DefaultCurve() => new AnimationCurve(
            new Keyframe(0f, 0.35f), new Keyframe(0.15f, 0.7f), new Keyframe(0.30f, 0.95f),
            new Keyframe(0.42f, 0.5f), new Keyframe(0.60f, 1.05f), new Keyframe(0.72f, 0.55f),
            new Keyframe(0.85f, 1.1f), new Keyframe(1f, 1.15f));

        private static PhaseSpan[] DefaultPhases() => new[]
        {
            new PhaseSpan(DirectorPhase.BuildUp, 0f), new PhaseSpan(DirectorPhase.Peak, 0.22f),
            new PhaseSpan(DirectorPhase.Breather, 0.36f), new PhaseSpan(DirectorPhase.HarderPeak, 0.48f),
            new PhaseSpan(DirectorPhase.Breather, 0.68f), new PhaseSpan(DirectorPhase.Finale, 0.78f),
        };
    }
}
