using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>Fits a new car's stay: jittered per car so returns spread out, then cut for a late arrival.</summary>
    public static class ArrivalStay
    {
        /// <summary>
        /// The stay is jittered per car, then a late arrival's stay is cut so its patron is back asking for the car before
        /// the shift ends (the cut comes last, so the stay always fits the shift).
        /// </summary>
        public static PatronRequest Fit(PatronRequest request, CarId car, PatronTuning tuning, float secondsRemaining)
        {
            var remaining = secondsRemaining * tuning.MinutesPerSecond;
            var spread = StayJitter.Apply(request.StayMinutes, tuning.StayJitter, tuning.LateStayFloor, car.Value);
            var stay = StayFit.Clamp(spread, remaining, tuning.LateStayLead, tuning.LateStayFloor);
            return stay != request.StayMinutes ? request.WithStay(stay) : request;
        }
    }
}
