using System;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Tunables for cars that drive themselves in to a stop and out of view. Lengths in world units.</summary>
    [Serializable]
    public sealed class CarAutoDriveStyle
    {
        [SerializeField, Min(1f), Tooltip("How far behind the stop point an arriving car starts (off screen).")]
        private float _entryDistance = 14f;
        [SerializeField, Min(0.1f), Tooltip("Cruise speed while arriving.")]
        private float _arriveSpeed = 7f;
        [SerializeField, Min(0.1f), Tooltip("Braking rate while easing to the stop point.")]
        private float _arriveEase = 6f;
        [SerializeField, Min(1f), Tooltip("How far a leaving car drives before it is removed (off screen).")]
        private float _exitDistance = 16f;
        [SerializeField, Min(0.1f), Tooltip("Top speed while leaving.")]
        private float _exitSpeed = 10f;
        [SerializeField, Min(0.1f), Tooltip("Acceleration while leaving.")]
        private float _exitAcceleration = 8f;

        /// <summary>Distance behind the stop point where an arriving car starts.</summary>
        public float EntryDistance => _entryDistance;
        /// <summary>Cruise speed while arriving.</summary>
        public float ArriveSpeed => _arriveSpeed;
        /// <summary>Deceleration used to ease to a stop.</summary>
        public float ArriveEase => _arriveEase;
        /// <summary>Distance driven before a leaving car is removed.</summary>
        public float ExitDistance => _exitDistance;
        /// <summary>Top speed while leaving.</summary>
        public float ExitSpeed => _exitSpeed;
        /// <summary>Acceleration while leaving.</summary>
        public float ExitAcceleration => _exitAcceleration;
    }
}
