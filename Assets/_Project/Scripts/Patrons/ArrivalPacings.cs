using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>Picks the arrival pacing for a lot kind: crazy lots keep the queue full, every other lot keeps the director's pacing.</summary>
    public static class ArrivalPacings
    {
        public static IArrivalPacing For(LotKind kind, float topUpSpacingMinutes) =>
            kind == LotKind.Crazy ? (IArrivalPacing)new QueueTopUpPacing(topUpSpacingMinutes) : new DirectorArrivalPacing();
    }
}
