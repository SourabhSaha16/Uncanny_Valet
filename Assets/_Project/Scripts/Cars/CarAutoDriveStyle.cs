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
        [SerializeField, Min(0.1f), Tooltip("A delivered car takes the road exit only when it stands this close to the start of the exit route.")]
        private float _tripStartRadius = 3.5f;
        [SerializeField, Min(10f), Tooltip("Most degrees per second a car turns toward the road route's heading.")]
        private float _tripTurnRate = 300f;

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
        /// <summary>How close to the exit route's start a car must be to use the road exit.</summary>
        public float TripStartRadius => _tripStartRadius;
        /// <summary>Turn rate toward the road route heading, degrees per second.</summary>
        public float TripTurnRate => _tripTurnRate;
        /// <summary>Acceleration while leaving.</summary>
        public float ExitAcceleration => _exitAcceleration;
    }
}
