using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure rule for Perfect Park (car_handling_spec.md section 4): the car rests completely inside the bay box (the
    /// <see cref="BayFitRule"/> result, same tolerance as Parked Inside), aligned with the bay's long axis (nose in or tail in)
    /// and touching nothing. Where it sits inside the box does not matter. The angle limit comes from <see cref="PerfectTuning"/>.
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
        /// <param name="inside">The car is completely inside the bay box (CarParkedInside rule).</param>
        /// <param name="touching">The car touches a wall or another car.</param>
        public ParkVerdict Judge(float angleDegrees, bool inside, bool touching)
        {
            if (AxisDeviation(angleDegrees) > _tuning.ParkAngle) return ParkVerdict.Crooked;
            if (!inside) return ParkVerdict.OutsideBay;
            return touching ? ParkVerdict.Touching : ParkVerdict.Perfect;
        }
    }
}
