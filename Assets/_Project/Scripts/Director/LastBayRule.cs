using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>Exactly one EV bay Free and every other bay Working: send an EV, so the player must use the last bay now or save it.</summary>
    public sealed class LastBayRule : IDilemmaRule
    {
        private readonly DilemmaRuleSettings _settings;

        public LastBayRule(DilemmaRuleSettings settings) { _settings = settings; }

        public DilemmaKind Kind => DilemmaKind.LastBay;

        public DilemmaBoost? Evaluate(in LotSnapshot lot)
        {
            var total = DilemmaSolvable.SlotsOf(lot, StationKind.Charging);
            var free = lot.CountSlots(StationKind.Charging, StationSlotState.Free);
            var working = lot.CountSlots(StationKind.Charging, StationSlotState.Working);
            if (total < 2 || free != 1 || working != total - 1) return null;
            return new DilemmaBoost(CarArchetype.Ev, _settings.LastBay, DilemmaSolvable.Check(lot, _settings));
        }
    }
}
