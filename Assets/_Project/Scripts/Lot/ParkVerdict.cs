namespace UncannyValet.Lot
{
    /// <summary>Result of judging a car at rest in a bay; the first failing check is reported.</summary>
    public enum ParkVerdict
    {
        Perfect,
        Crooked,
        OffCentreSide,
        OffCentreLength,
        Touching,
    }
}
