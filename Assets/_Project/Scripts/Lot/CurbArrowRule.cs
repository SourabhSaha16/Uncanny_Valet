namespace UncannyValet.Lot
{
    /// <summary>Pure rule: the pickup arrow state from how many cars in the zone face as required and how many do not.</summary>
    public static class CurbArrowRule
    {
        /// <summary>Right when any car faces correctly, else Wrong when a car faces badly, else Idle.</summary>
        public static CurbArrowState Evaluate(int facingRight, int facingWrong)
        {
            if (facingRight > 0) return CurbArrowState.Right;
            return facingWrong > 0 ? CurbArrowState.Wrong : CurbArrowState.Idle;
        }
    }
}
