using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>
    /// The workload cost table (spec section 2). The serialized copy inside DirectorConfig is only ever read to seed
    /// ITuning; at runtime a separate instance is refreshed from the live tuning each tick.
    /// </summary>
    [Serializable]
    public sealed class WorkloadSettings
    {
        private const float DefaultWashMinutes = 15f;
        private const float DefaultMinutesPerSecond = 1f;

        [SerializeField, Min(1f), Tooltip("Seconds of owed work that make workload W = 1.0.")]
        private float _horizonSeconds = 30f;
        [SerializeField, Min(0f), Tooltip("Seconds to park a waiting car.")]
        private float _parkCost = 8f;
        [SerializeField, Min(0f), Tooltip("Seconds to deliver a requested pickup.")]
        private float _pickupCost = 10f;
        [SerializeField, Min(0f), Tooltip("Seconds to drive to the wash and back out, on top of the wash time left.")]
        private float _washTripCost = 6f;
        [SerializeField, Min(0f), Tooltip("Seconds to drive an EV to a bay (charging runs on its own).")]
        private float _chargeTripCost = 6f;
        [SerializeField, Min(0f), Tooltip("Seconds to move a finished or squatting car out of a station someone needs.")]
        private float _evictCost = 6f;
        [SerializeField, Min(0f), Tooltip("Seconds counted for a parked car whose pickup is about to land.")]
        private float _pickupSoonCost = 5f;
        [SerializeField, Min(0f), Tooltip("A parked car counts as 'pickup soon' when its patron returns within this many seconds.")]
        private float _pickupSoonSeconds = 20f;

        /// <summary>Seconds a full wash takes at the current clock; derived, never serialized.</summary>
        public float WashSecondsFull { get; private set; } = DefaultWashMinutes / DefaultMinutesPerSecond;

        public float HorizonSeconds => _horizonSeconds;
        public float ParkCost => _parkCost;
        public float PickupCost => _pickupCost;
        public float WashTripCost => _washTripCost;
        public float ChargeTripCost => _chargeTripCost;
        public float EvictCost => _evictCost;
        public float PickupSoonCost => _pickupSoonCost;
        public float PickupSoonSeconds => _pickupSoonSeconds;

        /// <summary>Writes the starting values into the live tuning (Bootstrap only).</summary>
        public void Seed(ITuning t)
        {
            t.Seed(TuningKey.DirHorizonSeconds, _horizonSeconds);
            t.Seed(TuningKey.DirCostPark, _parkCost);
            t.Seed(TuningKey.DirCostPickup, _pickupCost);
            t.Seed(TuningKey.DirCostWash, _washTripCost);
            t.Seed(TuningKey.DirCostCharge, _chargeTripCost);
            t.Seed(TuningKey.DirCostEvict, _evictCost);
            t.Seed(TuningKey.DirCostPickupSoon, _pickupSoonCost);
            t.Seed(TuningKey.DirPickupSoonSeconds, _pickupSoonSeconds);
        }

        /// <summary>Refreshes this runtime copy from the live tuning.</summary>
        public void Read(ITuning t)
        {
            _horizonSeconds = Mathf.Max(1f, TuningRead.Float(t, TuningKey.DirHorizonSeconds, _horizonSeconds));
            _parkCost = TuningRead.Float(t, TuningKey.DirCostPark, _parkCost);
            _pickupCost = TuningRead.Float(t, TuningKey.DirCostPickup, _pickupCost);
            _washTripCost = TuningRead.Float(t, TuningKey.DirCostWash, _washTripCost);
            _chargeTripCost = TuningRead.Float(t, TuningKey.DirCostCharge, _chargeTripCost);
            _evictCost = TuningRead.Float(t, TuningKey.DirCostEvict, _evictCost);
            _pickupSoonCost = TuningRead.Float(t, TuningKey.DirCostPickupSoon, _pickupSoonCost);
            _pickupSoonSeconds = TuningRead.Float(t, TuningKey.DirPickupSoonSeconds, _pickupSoonSeconds);
            var minutes = TuningRead.Float(t, TuningKey.WashMinutes, DefaultWashMinutes);
            var perSecond = TuningRead.Float(t, TuningKey.GameMinutesPerSecond, DefaultMinutesPerSecond);
            WashSecondsFull = perSecond > 0f ? minutes / perSecond : minutes;
        }
    }
}
