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

        [Header("Perfect park")]
        [SerializeField, Range(1f, 20f), Tooltip("Largest angle off the bay's long axis, nose in or tail in.")]
        private float _parkAngleDegrees = 12f;
        [SerializeField, Min(0.05f), Tooltip("Largest side-to-side offset from the bay centre line. Must stay inside the bay slack (0.8).")]
        private float _parkSideOffset = 0.35f;
        [SerializeField, Min(0.1f), Tooltip("Largest lengthwise offset from the bay centre. Must stay inside the bay slack (1.2).")]
        private float _parkLengthOffset = 0.7f;
        [SerializeField, Min(0f), Tooltip("Seconds before the same car can earn another Perfect Park.")]
        private float _parkCooldownSeconds = 10f;
        [SerializeField, Min(0f), Tooltip("Units a car must have moved since it last stopped for a Perfect Park to count again.")]
        private float _parkMinTravel = 3f;

        public float DropOffAngleDegrees => _dropOffAngleDegrees;
        public float DropOffTimerShare => _dropOffTimerShare;
        public float ParkAngleDegrees => _parkAngleDegrees;
        public float ParkSideOffset => _parkSideOffset;
        public float ParkLengthOffset => _parkLengthOffset;
        public float ParkCooldownSeconds => _parkCooldownSeconds;
        public float ParkMinTravel => _parkMinTravel;
    }
}
