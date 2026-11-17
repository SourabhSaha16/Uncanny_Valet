namespace UncannyValet.Lot
{
    /// <summary>
    /// The strip of the curb zone the two pickup arrows mark: from the left arrow's centre line to the right one's, measured
    /// sideways from the zone centre. A zone without arrows has no span, and then every position counts as between them.
    /// </summary>
    public readonly struct CurbArrowSpan
    {
        private readonly float _halfWidth;

        public CurbArrowSpan(float halfWidth)
        {
            _halfWidth = halfWidth > 0f ? halfWidth : 0f;
        }

        /// <summary>No arrows painted: nothing to rest between.</summary>
        public static CurbArrowSpan None => default;

        /// <summary>True when there are arrows to rest between.</summary>
        public bool IsMarked => _halfWidth > 0f;

        /// <summary>
        /// True when a car centre <paramref name="offsetX"/> to the side of the zone centre lies between the arrows, allowing
        /// <paramref name="tolerance"/> past each arrow's centre line (the edge counts as between).
        /// </summary>
        public bool Contains(float offsetX, float tolerance)
        {
            if (!IsMarked) return true;
            var limit = _halfWidth + (tolerance < 0f ? 0f : tolerance);
            return offsetX >= -limit && offsetX <= limit;
        }
    }
}
