using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Economy
{
    /// <summary>Base, speed bonus or wait penalty, charge and clean bonuses; $0 when forced. Never below 0.</summary>
    public sealed class TipCalculator : ITipCalculator
    {
        private readonly TipConfig _config;
        private readonly ITuning _tuning;

        public TipCalculator(TipConfig config, ITuning tuning = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _tuning = tuning;
        }

        public int Calculate(in TipInput input) => Explain(input).Total;

        public TipBreakdown Explain(in TipInput input)
        {
            if (input.PickupExpired) return Zeroed(TipZeroReason.PickupExpired);
            if (input.UncleanedDirty) return Zeroed(TipZeroReason.NotCleaned);
            var speed = SpeedAdjust(input);
            var baseTip = Live(TuningKey.BaseTip, input.BaseTip);
            var charge = input.ChargedEv ? Live(TuningKey.ChargeBonus, _config.ChargeBonus) : 0;
            var clean = input.CleanedDirty ? Live(TuningKey.CleanBonus, _config.CleanBonus) : 0;
            var total = Math.Max(0, baseTip + speed + charge + clean);
            return new TipBreakdown(baseTip, speed, charge, clean, total, TipZeroReason.None);
        }

        private int Live(TuningKey key, int configured)
            => _tuning != null && _tuning.TryGet(key, out var v) ? (int)Math.Round(v, MidpointRounding.AwayFromZero) : configured;

        private static TipBreakdown Zeroed(TipZeroReason reason) => new TipBreakdown(0, 0, 0, 0, 0, reason);

        private int SpeedAdjust(in TipInput input)
        {
            var window = _tuning != null && _tuning.TryGet(TuningKey.SpeedWindowMinutes, out var w) ? w : _config.SpeedBonusWindowMinutes;
            if (input.WaitMinutes <= window)
                return input.Impatient ? _config.ImpatientSpeedBonus : _config.SpeedBonus;
            var penalty = (input.WaitMinutes - window) * _config.PenaltyPerMinute;
            return -(int)Math.Round(penalty, MidpointRounding.AwayFromZero);
        }
    }
}
