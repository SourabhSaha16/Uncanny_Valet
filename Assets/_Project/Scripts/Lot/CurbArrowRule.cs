namespace UncannyValet.Lot
{
    /// <summary>Pure rule: the pickup arrow state from the cars in the zone.</summary>
    public static class CurbArrowRule
    {
        /// <summary>
        /// Right when a car faces correctly and its patron will take it, else Wrong when a car faces badly,
        /// else Idle (empty, or only correctly facing cars whose patron has not asked yet).
        /// </summary>
        public static CurbArrowState Evaluate(int accepted, int facingWrong)
        {
            if (accepted > 0) return CurbArrowState.Right;
            return facingWrong > 0 ? CurbArrowState.Wrong : CurbArrowState.Idle;
        }
    }
}
