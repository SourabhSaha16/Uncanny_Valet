using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>What the director brain reads from a lot's profile: the target curve, its phases and the base weights.</summary>
    public interface IIntensityProfile
    {
        /// <summary>The target workload T at a fraction (0..1) of the shift.</summary>
        float Sample(float shiftFraction);

        /// <summary>The phase at a fraction of the shift.</summary>
        DirectorPhase PhaseAt(float shiftFraction);

        /// <summary>The area under T from a fraction of the shift to its end, in workload-seconds.</summary>
        float AreaFrom(float shiftFraction, float shiftLengthSeconds);

        /// <summary>True when the target curve is climbing at this fraction of the shift.</summary>
        bool IsRising(float shiftFraction);

        /// <summary>The base archetype weights.</summary>
        ArchetypeWeights Weights { get; }
    }
}
