namespace UncannyValet.Lot
{
    /// <summary>Pure maths: where the pickup arrows sit so they stay clear of the vertical VALET PICKUP text.</summary>
    public static class CurbArrowLayout
    {
        /// <summary>
        /// X (from the zone centre, positive side) of the centre of the free strip between the outermost text column and
        /// the zone edge. The arrow is painted at plus and minus this value, so the pair is centred on the zone.
        /// </summary>
        public static float SideX(float zoneWidth, float textHalfExtent, float edgeInset)
        {
            var outer = zoneWidth * 0.5f - edgeInset;
            return (textHalfExtent + outer) * 0.5f;
        }

        /// <summary>Half the width of the text block: the outermost column centre plus half a letter.</summary>
        public static float TextHalfExtent(int columns, float spacing, float letterWidth)
            => (columns - 1) * 0.5f * spacing + letterWidth * 0.5f;
    }
}
