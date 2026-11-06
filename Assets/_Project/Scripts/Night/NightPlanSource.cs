using System;
using System.Collections.Generic;
using UnityEngine;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Night
{
    /// <summary>
    /// Builds each night's plan from the config and applies live tuning edits on top. An edit to a plan key stays
    /// in the override set for the current and every later night; setting a key back to its seeded value (a Reset) clears it.
    /// </summary>
    public sealed class NightPlanSource
    {
        private static readonly TuningKey[] PlanKeys =
        {
            TuningKey.PlanMaxWaitingCars, TuningKey.PlanArrivalGapScale, TuningKey.PlanActiveSpots,
            TuningKey.PlanEvChance, TuningKey.PlanDirtyChance, TuningKey.PlanLateEvChance, TuningKey.PlanLateEvStart,
        };

        private readonly NightPlanConfig _plans;
        private readonly Dictionary<TuningKey, float> _overrides = new Dictionary<TuningKey, float>();
        private readonly Dictionary<TuningKey, float> _seeds = new Dictionary<TuningKey, float>();

        /// <summary>plans may be null (built-in defaults); tuning may be null (no seeds known). Reads the seeded values now.</summary>
        public NightPlanSource(NightPlanConfig plans, ITuning tuning)
        {
            _plans = plans;
            if (tuning == null) return;
            foreach (var key in PlanKeys)
                if (tuning.TryGet(key, out var seed)) _seeds[key] = seed;
        }

        /// <summary>The 1-based night's plan with every override applied.</summary>
        public NightPlan For(int night)
        {
            var p = _plans != null ? _plans.For(night) : NightPlanDefaults.For(night);
            return new NightPlan(p.Night,
                Mathf.RoundToInt(Value(TuningKey.PlanMaxWaitingCars, p.MaxWaitingCars)),
                Value(TuningKey.PlanArrivalGapScale, p.ArrivalGapScale),
                Value(TuningKey.PlanEvChance, p.EvChance), Value(TuningKey.PlanDirtyChance, p.DirtyChance),
                p.ChargingOpen, p.WashOpen, p.NewTonight,
                Mathf.RoundToInt(Value(TuningKey.PlanActiveSpots, p.ActiveSpots)),
                Value(TuningKey.PlanLateEvChance, p.LateEvChance), Value(TuningKey.PlanLateEvStart, p.LateEvStart));
        }

        /// <summary>Records an edit. Returns true when the key is a plan key, so the running plan must be republished.</summary>
        public bool Record(TuningKey key, float value)
        {
            if (Array.IndexOf(PlanKeys, key) < 0) return false;
            if (_seeds.TryGetValue(key, out var seed) && Mathf.Approximately(seed, value)) _overrides.Remove(key);
            else _overrides[key] = value;
            return true;
        }

        private float Value(TuningKey key, float configured)
            => _overrides.TryGetValue(key, out var v) ? v : configured;
    }
}
