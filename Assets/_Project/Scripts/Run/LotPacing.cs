using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Run
{
    /// <summary>
    /// One lot's shift length and arrival pacing. These replace the shared defaults (ShiftConfig, PatronConfig) for
    /// the lot; everything else about pacing stays in the shared assets and the difficulty dials.
    /// </summary>
    [Serializable]
    public sealed class LotPacing
    {
        [SerializeField, Min(1f), Tooltip("Real seconds in the shift. Training 150, crazy 240.")]
        private float _shiftLengthSeconds = 150f;
        [SerializeField, Min(0.5f), Tooltip("Shortest gap between arrivals, in real seconds.")]
        private float _arrivalGapMinSeconds = 15f;
        [SerializeField, Min(0.5f), Tooltip("Longest gap between arrivals, in real seconds.")]
        private float _arrivalGapMaxSeconds = 25f;
        [SerializeField, Range(1, 4), Tooltip("Most cars waiting for a spot at once. The queue holds as many as the layout has slots; the diner has 3.")]
        private int _maxWaitingCars = 2;

        public float ShiftLengthSeconds => _shiftLengthSeconds;
        public float ArrivalGapMinSeconds => _arrivalGapMinSeconds;
        public float ArrivalGapMaxSeconds => _arrivalGapMaxSeconds;
        public int MaxWaitingCars => _maxWaitingCars;

        /// <summary>True when the shift is longer than zero, the gaps are positive and ordered, and at least one car may wait.</summary>
        public bool IsSane => _shiftLengthSeconds > 0f && _arrivalGapMinSeconds > 0f
            && _arrivalGapMaxSeconds >= _arrivalGapMinSeconds && _maxWaitingCars >= 1;

        /// <summary>The waiting cap the layout can actually hold: the lot's cap, never above its queue slots.</summary>
        public int EffectiveMaxWaiting(int queueSlots) => Mathf.Max(1, Mathf.Min(_maxWaitingCars, queueSlots));

        /// <summary>
        /// Seeds the lot's numbers into the tuning store. Gaps are stored in game minutes, so they are scaled by
        /// <paramref name="gameMinutesPerSecond"/>. Call before the difficulty dials capture their baselines.
        /// </summary>
        public void SeedInto(ITuning tuning, float gameMinutesPerSecond, int queueSlots)
        {
            tuning.Seed(TuningKey.ShiftLengthSeconds, _shiftLengthSeconds);
            tuning.Seed(TuningKey.ArrivalGapMinMinutes, _arrivalGapMinSeconds * gameMinutesPerSecond);
            tuning.Seed(TuningKey.ArrivalGapMaxMinutes, _arrivalGapMaxSeconds * gameMinutesPerSecond);
            tuning.Seed(TuningKey.PlanMaxWaitingCars, EffectiveMaxWaiting(queueSlots));
        }
    }
}
