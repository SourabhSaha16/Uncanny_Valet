using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Run
{
    /// <summary>How much of the shared power bar (0 to 100) each skill fills in this lot. Seeded over the shared power config.</summary>
    [Serializable]
    public sealed class LotPowerFill
    {
        [SerializeField, Range(0f, 100f), Tooltip("Power bar gained for one Perfect Park.")]
        private float _perfectPark = 18f;
        [SerializeField, Range(0f, 100f), Tooltip("Power bar gained for one perfect drop-off.")]
        private float _perfectDropOff = 10f;
        [SerializeField, Range(0f, 5f), Tooltip("Power bar gained per second of sliding.")]
        private float _driftPerSecond = 0.6f;
        [SerializeField, Range(0f, 20f), Tooltip("Most power bar one slide can add.")]
        private float _driftCap = 4f;

        public float PerfectPark => _perfectPark;
        public float PerfectDropOff => _perfectDropOff;
        public float DriftPerSecond => _driftPerSecond;
        public float DriftCap => _driftCap;

        /// <summary>True when no number is negative.</summary>
        public bool IsSane => _perfectPark >= 0f && _perfectDropOff >= 0f && _driftPerSecond >= 0f && _driftCap >= 0f;

        /// <summary>Writes the lot's fill amounts into the tuning store.</summary>
        public void SeedInto(ITuning tuning)
        {
            tuning.Seed(TuningKey.PowerFillPerfectPark, _perfectPark);
            tuning.Seed(TuningKey.PowerFillPerfectDropOff, _perfectDropOff);
            tuning.Seed(TuningKey.PowerFillDriftPerSecond, _driftPerSecond);
            tuning.Seed(TuningKey.PowerFillDriftCap, _driftCap);
        }
    }
}
