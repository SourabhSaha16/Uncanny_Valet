using System;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Tunable number for the "car left the queue" rule (issue 78); seeded into the tuning store at startup.</summary>
    [Serializable]
    public sealed class QueueClearSettings
    {
        [SerializeField, Min(1f), Tooltip("World units from the nearest queue slot centre at which a resting car counts as clear of the queue. The queue slots are about 4.7 apart, so 6 is clear of the last slot by more than one car length.")]
        private float _clearDistance = 6f;

        public float ClearDistance => _clearDistance;
    }
}
