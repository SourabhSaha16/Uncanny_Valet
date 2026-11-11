using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>
    /// Pure rule: turns the eight difficulty dials into every underlying tuning key they drive, by piecewise-linear
    /// interpolation through the config's anchors (easy, default, hard). Each key keeps its starting value (captured
    /// once, after seeding), so the default dial positions reproduce the authored numbers exactly.
    /// </summary>
    public sealed class DifficultyDials
    {
        /// <summary>The dial keys, in panel order. (The seed dial drives nothing and is not listed.)</summary>
        public static readonly TuningKey[] DialKeys =
        {
            TuningKey.DialIntensity, TuningKey.DialRamp, TuningKey.DialLateRush, TuningKey.DialDirtyCars,
            TuningKey.DialEvCars, TuningKey.DialDilemmas, TuningKey.DialMercy, TuningKey.DialSkillReward,
        };

        private readonly float _min;
        private readonly float _default;
        private readonly float _max;
        private readonly Dictionary<TuningKey, float> _start = new Dictionary<TuningKey, float>();
        private readonly Dictionary<TuningKey, List<DialLink>> _byKey = new Dictionary<TuningKey, List<DialLink>>();
        private readonly Dictionary<TuningKey, float> _baseline = new Dictionary<TuningKey, float>();
        private readonly Dictionary<TuningKey, float> _positions = new Dictionary<TuningKey, float>();
        private readonly Dictionary<TuningKey, TuningInfo> _info = new Dictionary<TuningKey, TuningInfo>();

        /// <summary>Copies the anchors out of the config; the config can be destroyed afterwards.</summary>
        public DifficultyDials(DifficultyDialsConfig config)
        {
            _min = config.MinPosition; _default = config.DefaultPosition; _max = config.MaxPosition;
            foreach (var link in config.Links)
            {
                if (!_byKey.TryGetValue(link.Key, out var list)) _byKey[link.Key] = list = new List<DialLink>();
                list.Add(link);
            }
            foreach (var dial in DialKeys) _start[dial] = config.StartOf(dial);
            foreach (var info in TuningCatalog.All) _info[info.Key] = info;
        }

        /// <summary>True for the dial keys.</summary>
        public static bool IsDial(TuningKey key) => System.Array.IndexOf(DialKeys, key) >= 0;

        /// <summary>Seeds every dial at its starting (baseline) position. Bootstrap only.</summary>
        public void SeedDials(ITuning tuning)
        {
            for (var i = 0; i < DialKeys.Length; i++) tuning.Seed(DialKeys[i], _start[DialKeys[i]]);
        }

        /// <summary>
        /// Seeds every driven key at the value its dial positions derive, so Reset returns to the baseline. Call after
        /// <see cref="CaptureBaselines"/>; the starting values stay the authored ones.
        /// </summary>
        public void SeedDerived(ITuning tuning)
        {
            ReadPositions(tuning);
            foreach (var pair in _baseline) tuning.Seed(pair.Key, Derive(pair.Key, pair.Value));
        }

        /// <summary>Remembers each driven key's current value as its starting value. Call after seeding, before any edit.</summary>
        public void CaptureBaselines(ITuning tuning)
        {
            _baseline.Clear();
            foreach (var pair in _byKey)
                if (tuning.TryGet(pair.Key, out var value)) _baseline[pair.Key] = value;
        }

        /// <summary>Re-derives every driven key from the current dial values and writes the changed ones.</summary>
        public void Apply(ITuning tuning)
        {
            ReadPositions(tuning);
            foreach (var pair in _baseline)
            {
                var derived = Derive(pair.Key, pair.Value);
                if (!tuning.TryGet(pair.Key, out var current) || !Mathf.Approximately(current, derived)) tuning.Set(pair.Key, derived);
            }
        }

        private void ReadPositions(ITuning tuning)
        {
            for (var i = 0; i < DialKeys.Length; i++)
                _positions[DialKeys[i]] = tuning.TryGet(DialKeys[i], out var v) ? Mathf.Clamp(v, _min, _max) : _default;
        }

        /// <summary>The value a key takes at the dial positions last read by <see cref="Apply"/>.</summary>
        private float Derive(TuningKey key, float start)
        {
            var scale = 1f;
            var add = 0f;
            var pivot = 0f;
            var links = _byKey[key];
            for (var i = 0; i < links.Count; i++)
            {
                var anchor = Interpolate(links[i], _positions[links[i].Dial]);
                if (links[i].Effect == DialEffect.Add) add += anchor;
                else { scale *= anchor; pivot = links[i].Pivot; }
            }
            if (scale == 1f && add == 0f) return start; // the default position changes nothing, bit for bit
            return Constrain(key, start, pivot + (start - pivot) * scale + add);
        }

        // Piecewise linear: min..default, then default..max. A knot returns its anchor exactly.
        private float Interpolate(in DialLink link, float position)
        {
            if (position <= _default)
            {
                var t = (position - _min) / (_default - _min);
                return link.AtMin * (1f - t) + link.AtDefault * t;
            }
            var u = (position - _default) / (_max - _default);
            return link.AtDefault * (1f - u) + link.AtMax * u;
        }

        // Whole-step keys round; every key stays in its panel range (widened to include its starting value).
        private float Constrain(TuningKey key, float start, float value)
        {
            if (!_info.TryGetValue(key, out var info)) return value;
            if (info.Step >= 1f) value = Mathf.Round(value);
            return Mathf.Clamp(value, Mathf.Min(info.Min, start), Mathf.Max(info.Max, start));
        }
    }
}
