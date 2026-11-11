using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure rule for Perfect Park (car_handling_spec.md section 4): the car rests aligned with the bay's long axis (nose in or
    /// tail in), close to the centre line and centre point, touching nothing. Limits come from <see cref="PerfectTuning"/>.
    /// </summary>
    public sealed class ParkJudge
    {
        private const float QuarterTurn = 90f;
        private const float HalfTurn = 180f;

        private readonly PerfectTuning _tuning;

        public ParkJudge(PerfectTuning tuning) { _tuning = tuning; }

        /// <summary>Smallest angle between a rotation relative to the bay and the bay's long axis (either direction), 0..90.</summary>
        public static float AxisDeviation(float relativeDegrees)
        {
            var d = Mathf.Abs(Mathf.DeltaAngle(0f, relativeDegrees));
            return d > QuarterTurn ? HalfTurn - d : d;
        }

        /// <param name="angleDegrees">Car rotation relative to the bay (0 = nose along the bay's up axis).</param>
        /// <param name="sideOffset">Distance of the car centre from the bay centre line.</param>
        /// <param name="lengthOffset">Distance of the car centre from the bay centre along the bay.</param>
        /// <param name="touching">The car touches a wall or another car.</param>
        public ParkVerdict Judge(float angleDegrees, float sideOffset, float lengthOffset, bool touching)
        {
            if (AxisDeviation(angleDegrees) > _tuning.ParkAngle) return ParkVerdict.Crooked;
            if (Mathf.Abs(sideOffset) > _tuning.ParkSideOffset) return ParkVerdict.OffCentreSide;
            if (Mathf.Abs(lengthOffset) > _tuning.ParkLengthOffset) return ParkVerdict.OffCentreLength;
            return touching ? ParkVerdict.Touching : ParkVerdict.Perfect;
        }
    }
}
