namespace UncannyValet.Lot
{
    /// <summary>Pure rule: which spot ids are open for a night plan's ActiveSpots value.</summary>
    public static class OpenSpotRule
    {
        /// <summary>Highest open spot id. 0 or less means every spot is open.</summary>
        public static int Limit(int activeSpots) => activeSpots <= 0 ? int.MaxValue : activeSpots;
    }
}
