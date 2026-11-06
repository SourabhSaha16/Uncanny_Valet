namespace UncannyValet.Patrons
{
    /// <summary>
    /// Everything that defines one patron visit. The spawner and story cards both build one of these
    /// and hand it to <see cref="PatronSpawner.TrySpawn"/>.
    /// </summary>
    public readonly struct PatronRequest
    {
        /// <summary>Default park window in game minutes (also the PatronConfig default).</summary>
        public const float DefaultParkWindowMinutes = 45f;

        /// <summary>Default pickup window in game minutes (also the PatronConfig default).</summary>
        public const float DefaultPickupWindowMinutes = 60f;

        public readonly float StayMinutes;
        public readonly int BaseTip;
        public readonly bool Impatient;
        public readonly float ParkWindowMinutes;
        public readonly float PickupWindowMinutes;

        public PatronRequest(float stayMinutes, int baseTip, bool impatient,
            float parkWindowMinutes = DefaultParkWindowMinutes,
            float pickupWindowMinutes = DefaultPickupWindowMinutes)
        {
            StayMinutes = stayMinutes; BaseTip = baseTip; Impatient = impatient;
            ParkWindowMinutes = parkWindowMinutes; PickupWindowMinutes = pickupWindowMinutes;
        }
    }
}
