namespace UncannyValet.Lot
{
    /// <summary>Result of judging a car at rest in a bay; the first failing check is reported (heading, then the bay box, then contact).</summary>
    public enum ParkVerdict
    {
        Perfect,
        Crooked,
        OutsideBay,
        Touching,
    }
}
