using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Economy
{
    /// <summary>
    /// Base, speed bonus or wait penalty, patience, and modifier-clear bonuses, then less damage, then times the
    /// favour, then halved (ties up) once when any needed service modifier is still open. 0 when forced. Never below 0.
    /// </summary>
    public sealed class BlessingCalculator : IBlessingCalculator
    {
        private readonly BlessingConfig _config;
        private readonly ITuning _tuning;
        private readonly IModifierCatalog _catalog;

        /// <param name="catalog">The modifier definitions that give each cleared modifier its bonus; null pays no modifier bonus.</param>
        public BlessingCalculator(BlessingConfig config, ITuning tuning = null, IModifierCatalog catalog = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _tuning = tuning;
            _catalog = catalog;
        }

        public int Calculate(in BlessingInput input) => Explain(input).Total;

        public BlessingBreakdown Explain(in BlessingInput input)
        {
            if (input.PickupExpired) return Zeroed(BlessingZeroReason.PickupExpired);
            if (DamagePenalty.IsWrecked(input.Damage01)) return Zeroed(BlessingZeroReason.Wrecked);
            var speed = SpeedAdjust(input);
            var baseBlessing = Live(TuningKey.BaseBlessing, input.BaseBlessing);
            var modifiers = ModifierBonus(input.ClearedModifiers);
            var patience = PatienceBonus(input);
            var subtotal = Math.Max(0, baseBlessing + speed + modifiers + patience);
            var damage = DamageAdjust(input, subtotal);
            var favour = Math.Max(1, input.FavourMultiplier);
            var total = Math.Max(0, subtotal + damage) * favour;
            var missed = !input.OpenModifiers.IsEmpty;
            if (missed) total = HalveRoundingUp(total);
            return new BlessingBreakdown(baseBlessing, speed, modifiers, total, BlessingZeroReason.None, patience, damage, favour, missed);
        }

        // Each cleared modifier pays its definition's bonus, or the live value of the key that bonus is tuned through.
        private int ModifierBonus(ModifierSet cleared)
        {
            if (_catalog == null) return 0;
            var sum = 0;
            for (var i = 0; i < cleared.Count; i++)
                if (_catalog.TryGet(cleared.At(i), out var def)) sum += Live(def.BonusKey, def.Bonus);
            return sum;
        }

        // A missed service halves the final amount once, to the nearest whole blessing with ties up (5 pays 3).
        private static int HalveRoundingUp(int total) => (total + 1) / 2;

        private int Live(TuningKey key, int configured)
            => _tuning != null && _tuning.TryGet(key, out var v) ? (int)Math.Round(v, MidpointRounding.AwayFromZero) : configured;

        private static BlessingBreakdown Zeroed(BlessingZeroReason reason) => new BlessingBreakdown(0, 0, 0, 0, reason);

        private int PatienceBonus(in BlessingInput input)
        {
            var left = Math.Min(1f, Math.Max(0f, input.PatienceRemaining01));
            return (int)Math.Round(left * _config.MaxPatienceBonus, MidpointRounding.AwayFromZero);
        }

        private int DamageAdjust(in BlessingInput input, int subtotal)
        {
            var share = DamagePenalty.Share(input.Damage01, _config.DamageStageSize, _config.DamagePenaltyPerStage);
            return -(int)Math.Round(subtotal * share, MidpointRounding.AwayFromZero);
        }

        private int SpeedAdjust(in BlessingInput input)
        {
            var window = _tuning != null && _tuning.TryGet(TuningKey.SpeedWindowMinutes, out var w) ? w : _config.SpeedBonusWindowMinutes;
            if (input.WaitMinutes <= window)
                return input.Impatient ? _config.ImpatientSpeedBonus : _config.SpeedBonus;
            var penalty = (input.WaitMinutes - window) * _config.PenaltyPerMinute;
            return -(int)Math.Round(penalty, MidpointRounding.AwayFromZero);
        }
    }
}
