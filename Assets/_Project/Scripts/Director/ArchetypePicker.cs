using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>
    /// Pure weighted draw of the next car's archetype (spec section 4): the lot's base weights for the stations it has
    /// unlocked, scaled by the pacing band, multiplied by the solvable dilemma rules when the budget allows, then
    /// limited by the streak and mix rules and drawn with the shift's one seeded random source. In relief only a
    /// plain car is possible. Allocation-free after construction.
    /// </summary>
    public sealed class ArchetypePicker
    {
        private readonly IDirectorRandom _random;
        private readonly IReadOnlyList<IDilemmaRule> _rules;
        private readonly PickerSettings _settings;
        private readonly ArchetypeHistory _history = new ArchetypeHistory();
        private readonly float[] _base = new float[ArchetypeKinds.Count];
        private readonly float[] _scaled = new float[ArchetypeKinds.Count];
        private readonly float[] _limited = new float[ArchetypeKinds.Count];
        private readonly float[] _bestBoost = new float[ArchetypeKinds.Count];
        private readonly DilemmaKind[] _bestRule = new DilemmaKind[ArchetypeKinds.Count];

        public ArchetypePicker(IDirectorRandom random, IReadOnlyList<IDilemmaRule> rules, PickerSettings settings)
        {
            _random = random; _rules = rules; _settings = settings;
        }

        /// <summary>What has been picked this shift (counts and streaks).</summary>
        public ArchetypeHistory History => _history;

        /// <summary>Forgets the shift's picks.</summary>
        public void Reset() => _history.Reset();

        /// <summary>Draws the next archetype.</summary>
        public PickResult Pick(in PickRequest request)
        {
            FillUnlocked(request);
            if (request.Relief) return Finish(CarArchetype.Plain, false);
            for (var i = 0; i < _scaled.Length; i++) _scaled[i] = _base[i];
            ScaleForBand(request.Band);
            var earlyReturn = request.DilemmaAllowed && ApplyRules(request.Lot);
            if (!Limit(request.Budget, true) && !Limit(request.Budget, false)) return Finish(CarArchetype.Plain, earlyReturn);
            return Finish(Draw(), earlyReturn);
        }

        // An archetype is drawable only when the lot has unlocked the stations it needs (a slot of that kind exists).
        private void FillUnlocked(in PickRequest r)
        {
            var wash = DilemmaSolvable.SlotsOf(r.Lot, StationKind.Wash) > 0;
            var charge = DilemmaSolvable.SlotsOf(r.Lot, StationKind.Charging) > 0;
            for (var i = 0; i < _base.Length; i++)
            {
                var a = (CarArchetype)i;
                var open = (!ArchetypeKinds.NeedsWash(a) || wash) && (!ArchetypeKinds.NeedsCharge(a) || charge);
                _base[i] = open ? r.Weights.Of(a) : 0f;
                _bestBoost[i] = 1f;
            }
        }

        private void ScaleForBand(ScheduleReason band)
        {
            var under = band == ScheduleReason.UnderTarget || band == ScheduleReason.Idle;
            var over = band == ScheduleReason.OverTarget;
            if (!under && !over) return;
            var heavy = under ? _settings.UnderHeavy : _settings.OverHeavy;
            _scaled[(int)CarArchetype.Dirty] *= heavy;
            _scaled[(int)CarArchetype.DirtyEv] *= heavy;
            if (over) _scaled[(int)CarArchetype.Plain] *= _settings.OverPlain;
        }

        // Multiplies in every triggered, solvable rule. Returns true when one of them wants an early return.
        private bool ApplyRules(in LotSnapshot lot)
        {
            var earlyReturn = false;
            for (var i = 0; i < _rules.Count; i++)
            {
                var boost = _rules[i].Evaluate(lot);
                if (!boost.HasValue || !boost.Value.Solvable) continue;
                var b = boost.Value;
                var m = _settings.Assist ? 1f + (b.Multiplier - 1f) * _settings.AssistDilemmaScale : b.Multiplier;
                earlyReturn |= b.BringReturnForward;
                if (m <= 1f) continue;
                var index = (int)b.Archetype;
                _scaled[index] *= m;
                if (m > _bestBoost[index]) { _bestBoost[index] = m; _bestRule[index] = _rules[i].Kind; }
            }
            return earlyReturn;
        }

        // Applies the streak limit and (optionally) the mix bounds. False when nothing is left to draw.
        private bool Limit(int budget, bool withMix)
        {
            for (var i = 0; i < _limited.Length; i++) _limited[i] = _scaled[i];
            if (withMix) MixLimit.Apply(_limited, _base, _history, budget, _settings.MixTolerance, _settings.StreakLimit);
            for (var i = 0; i < _limited.Length; i++)
            {
                var a = (CarArchetype)i;
                var dirtyRun = ArchetypeKinds.NeedsWash(a) && _history.DirtyStreak >= _settings.StreakLimit;
                var evRun = ArchetypeKinds.NeedsCharge(a) && _history.EvStreak >= _settings.StreakLimit;
                if (dirtyRun || evRun) _limited[i] = 0f;
            }
            return Sum() > 0f;
        }

        private float Sum()
        {
            var sum = 0f;
            for (var i = 0; i < _limited.Length; i++) sum += _limited[i];
            return sum;
        }

        private CarArchetype Draw()
        {
            var roll = _random.Value() * Sum();
            var last = CarArchetype.Plain;
            for (var i = 0; i < _limited.Length; i++)
            {
                if (_limited[i] <= 0f) continue;
                last = (CarArchetype)i;
                roll -= _limited[i];
                if (roll < 0f) return last;
            }
            return last;
        }

        private PickResult Finish(CarArchetype archetype, bool earlyReturn)
        {
            var index = (int)archetype;
            var boosted = _bestBoost[index] > 1f;
            _history.Record(archetype);
            return new PickResult(archetype, boosted, boosted ? _bestRule[index] : default, _bestBoost[index], earlyReturn);
        }
    }
}
