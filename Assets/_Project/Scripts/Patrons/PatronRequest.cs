namespace UncannyValet.Patrons
{
    /// <summary>
    /// Everything that defines one patron visit. The spawner and story cards both build one of these
    /// and hand it to <see cref="PatronSpawner.Spawn"/>.
    /// </summary>
    public readonly struct PatronRequest
    {
        public readonly float StayMinutes;
        public readonly int BaseTip;
        public readonly bool Impatient;

        public PatronRequest(float stayMinutes, int baseTip, bool impatient)
        {
            StayMinutes = stayMinutes; BaseTip = baseTip; Impatient = impatient;
        }
    }
}
