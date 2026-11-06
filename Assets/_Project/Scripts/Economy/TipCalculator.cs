using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Economy
{
    /// <summary>Base, speed bonus or wait penalty, charge and clean bonuses; $0 when forced. Never below 0.</summary>
    public sealed class TipCalculator : ITipCalculator
    {
        private readonly TipConfig _config;

        public TipCalculator(TipConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public int Calculate(in TipInput input) => Explain(input).Total;

        public TipBreakdown Explain(in TipInput input)
        {
            if (input.PickupExpired) return Zeroed(TipZeroReason.PickupExpired);
            if (input.UncleanedDirty) return Zeroed(TipZeroReason.NotCleaned);
            var speed = SpeedAdjust(input);
            var charge = input.ChargedEv ? _config.ChargeBonus : 0;
            var clean = input.CleanedDirty ? _config.CleanBonus : 0;
            var total = Math.Max(0, input.BaseTip + speed + charge + clean);
            return new TipBreakdown(input.BaseTip, speed, charge, clean, total, TipZeroReason.None);
        }

        private static TipBreakdown Zeroed(TipZeroReason reason) => new TipBreakdown(0, 0, 0, 0, 0, reason);

        private int SpeedAdjust(in TipInput input)
        {
            var window = _config.SpeedBonusWindowMinutes;
            if (input.WaitMinutes <= window)
                return input.Impatient ? _config.ImpatientSpeedBonus : _config.SpeedBonus;
            var penalty = (input.WaitMinutes - window) * _config.PenaltyPerMinute;
            return -(int)Math.Round(penalty, MidpointRounding.AwayFromZero);
        }
    }
}
