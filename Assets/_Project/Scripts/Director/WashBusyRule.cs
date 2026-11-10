using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>Wash slot Working and no dirty car waiting for it: send a Dirty car, so the player must queue it or park it and come back.</summary>
    public sealed class WashBusyRule : IDilemmaRule
    {
        private readonly DilemmaRuleSettings _settings;

        public WashBusyRule(DilemmaRuleSettings settings) { _settings = settings; }

        public DilemmaKind Kind => DilemmaKind.WashBusy;

        public DilemmaBoost? Evaluate(in LotSnapshot lot)
        {
            if (lot.CountSlots(StationKind.Wash, StationSlotState.Working) == 0 || WashQueued(lot)) return null;
            return new DilemmaBoost(CarArchetype.Dirty, _settings.WashBusy, DilemmaSolvable.Check(lot, _settings));
        }

        private static bool WashQueued(in LotSnapshot lot)
        {
            for (var i = 0; i < lot.Cars.Count; i++)
                if (lot.Cars[i].AwaitingPark && lot.Cars[i].NeedsWash) return true;
            return false;
        }
    }
}
