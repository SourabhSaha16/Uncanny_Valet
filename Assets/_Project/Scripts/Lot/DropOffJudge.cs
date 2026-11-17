namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure rule for a perfect drop-off (valet_powers.md Q1): the car faces up within a tight angle, rests inside the
    /// pickup zone between its two arrows, and arrives before the patron's pickup timer is part gone. All three limits come from <see cref="PerfectTuning"/>.
    /// </summary>
    public sealed class DropOffJudge
    {
        private readonly PerfectTuning _tuning;

        public DropOffJudge(PerfectTuning tuning) { _tuning = tuning; }

        /// <param name="rotationDegrees">Car rotation (0 = nose up).</param>
        /// <param name="insideZone">The car is settled inside the curb zone.</param>
        /// <param name="pickupElapsed01">Share 0..1 of the pickup timer already used.</param>
        /// <param name="offsetX">Sideways distance of the car centre from the zone centre.</param>
        /// <param name="span">The strip between the curb arrows; the car centre must lie in it (plus the tuned tolerance).</param>
        public bool IsPerfect(float rotationDegrees, bool insideZone, float pickupElapsed01, float offsetX, CurbArrowSpan span) =>
            insideZone
            && CurbFacingRule.DeviationFromRequired(rotationDegrees) <= _tuning.DropOffAngle
            && pickupElapsed01 < _tuning.DropOffTimerShare
            && span.Contains(offsetX, _tuning.DropOffArrowTolerance);
    }
}
