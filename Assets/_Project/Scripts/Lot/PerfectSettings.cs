using System;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Starting values of the Perfect Park and Perfect drop-off thresholds (part of <see cref="LotConfig"/>; read-only at runtime).</summary>
    [Serializable]
    public sealed class PerfectSettings
    {
        [Header("Perfect drop-off")]
        [SerializeField, Range(1f, 80f), Tooltip("Largest angle off straight down for a perfect drop-off (the normal rule allows 80).")]
        private float _dropOffAngleDegrees = 15f;
        [SerializeField, Range(0.05f, 1f), Tooltip("The car must arrive before this share of the patron's pickup timer is gone.")]
        private float _dropOffTimerShare = 0.5f;

        [SerializeField, Min(0f), Tooltip("How far the car centre may sit past either curb arrow's centre line sideways and still count as between the arrows.")]
        private float _dropOffArrowTolerance = 0.25f;

        [Header("Perfect park")]
        [SerializeField, Range(1f, 20f), Tooltip("Largest angle off the bay's long axis, nose in or tail in. The car must also be completely inside the bay box (see Parked inside).")]
        private float _parkAngleDegrees = 12f;
        [SerializeField, Min(0f), Tooltip("Seconds before the same car can earn another Perfect Park.")]
        private float _parkCooldownSeconds = 10f;
        [SerializeField, Min(0f), Tooltip("Units a car must have moved since it last stopped for a Perfect Park to count again.")]
        private float _parkMinTravel = 3f;

        [Header("Parked inside")]
        [SerializeField, Min(0f), Tooltip("How far the car collider may stray past the bay edge and still count as parked completely inside.")]
        private float _parkedInsideTolerance = 0.05f;

        public float ParkedInsideTolerance => _parkedInsideTolerance;
        public float DropOffAngleDegrees => _dropOffAngleDegrees;
        public float DropOffTimerShare => _dropOffTimerShare;
        public float DropOffArrowTolerance => _dropOffArrowTolerance;
        public float ParkAngleDegrees => _parkAngleDegrees;
        public float ParkCooldownSeconds => _parkCooldownSeconds;
        public float ParkMinTravel => _parkMinTravel;
    }
}
