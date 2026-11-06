using System.Collections.Generic;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Plain implementation of ITuning: a live value and a seeded default per key.</summary>
    public sealed class TuningStore : ITuning
    {
        private readonly IEventBus _bus;
        private readonly Dictionary<TuningKey, float> _values = new Dictionary<TuningKey, float>();
        private readonly Dictionary<TuningKey, float> _seeds = new Dictionary<TuningKey, float>();

        public TuningStore(IEventBus bus) { _bus = bus; }

        public float Get(TuningKey key)
        {
            if (_values.TryGetValue(key, out var v)) return v;
            Debug.LogError($"Tuning key {key} was never seeded. Bootstrap must call ITuning.Seed for it.");
            return 0f;
        }

        public bool TryGet(TuningKey key, out float value) => _values.TryGetValue(key, out value);

        public void Set(TuningKey key, float value)
        {
            _values[key] = value;
            _bus.Publish(new TuningChanged(key, value));
        }

        public void Seed(TuningKey key, float value)
        {
            _values[key] = value;
            _seeds[key] = value;
        }

        public void Reset(TuningKey key)
        {
            if (_seeds.TryGetValue(key, out var seed)) Set(key, seed);
        }
    }
}
