using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Economy
{
    /// <summary>
    /// Base, speed bonus or wait penalty, patience, charge, clean and Perfect Park bonuses, then less damage, then times the
    /// combo. $0 when forced. Never below 0.
    /// </summary>
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
            if (DamagePenalty.IsWrecked(input.Damage01)) return Zeroed(TipZeroReason.Wrecked);
            var speed = SpeedAdjust(input);
            var baseTip = Live(TuningKey.BaseTip, input.BaseTip);
            var charge = input.ChargedEv ? Live(TuningKey.ChargeBonus, _config.ChargeBonus) : 0;
            var clean = input.CleanedDirty ? Live(TuningKey.CleanBonus, _config.CleanBonus) : 0;
            var patience = PatienceBonus(input);
            var perfect = input.PerfectParks * Live(TuningKey.PerfectParkBonus, _config.PerfectParkBonus);
            var subtotal = Math.Max(0, baseTip + speed + charge + clean + patience + perfect);
            var damage = DamageAdjust(input, subtotal);
            var combo = Math.Max(1, input.ComboMultiplier);
            var total = Math.Max(0, subtotal + damage) * combo;
            return new TipBreakdown(baseTip, speed, charge, clean, total, TipZeroReason.None, patience, damage, combo, perfect);
        }

        private int Live(TuningKey key, int configured)
            => _tuning != null && _tuning.TryGet(key, out var v) ? (int)Math.Round(v, MidpointRounding.AwayFromZero) : configured;

        private static TipBreakdown Zeroed(TipZeroReason reason) => new TipBreakdown(0, 0, 0, 0, 0, reason);

        private int PatienceBonus(in TipInput input)
        {
            var left = Math.Min(1f, Math.Max(0f, input.PatienceRemaining01));
            return (int)Math.Round(left * _config.MaxPatienceBonus, MidpointRounding.AwayFromZero);
        }

        private int DamageAdjust(in TipInput input, int subtotal)
        {
            var share = DamagePenalty.Share(input.Damage01, _config.DamageStageSize, _config.DamagePenaltyPerStage);
            return -(int)Math.Round(subtotal * share, MidpointRounding.AwayFromZero);
        }

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
