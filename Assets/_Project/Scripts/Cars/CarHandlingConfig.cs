using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Handling numbers of docs/redesign/car_handling_spec.md sections 2 and 3: drift, grip, steering and the turning
    /// radius derived from the real aisle. Read-only at runtime; <see cref="CarHandlingLive"/> holds the live copy the F2 panel edits.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Car Handling Config", fileName = "CarHandlingConfig")]
    public sealed class CarHandlingConfig : ScriptableObject
    {
        [Header("Drift (section 2)")]
        [SerializeField, Min(0f), Tooltip("Sideways slide decays at this rate per second at full grip.")]
        private float _lateralFriction = 8f;
        [SerializeField, Range(0f, 1f), Tooltip("Share of the removed sideways speed handed back as forward speed (0 scrubs speed, 1 keeps momentum).")]
        private float _forwardGrip = 0.6f;
        [SerializeField, Min(0f), Tooltip("Speed (units/s) up to which grip is full.")]
        private float _gripLowSpeed = 3f;
        [SerializeField, Min(0f), Tooltip("Speed (units/s) from which grip is GripAtSpeed.")]
        private float _gripHighSpeed = 6f;
        [SerializeField, Range(0.01f, 1f), Tooltip("Grip fraction at and above the high speed.")]
        private float _gripAtSpeed = 0.55f;
        [SerializeField, Range(0.01f, 1f), Tooltip("Grip multiplier while the drift button (Shift) is held.")]
        private float _driftGripScale = 0.25f;
        [SerializeField, Min(0f), Tooltip("The drift button only has an effect above this speed (units/s).")]
        private float _driftMinSpeed = 1f;

        [Header("Turning radius (section 3)")]
        [SerializeField, Min(0.1f), Tooltip("Speed (units/s) at and above which steering has full effect; below it the turn rate scales down.")]
        private float _turnFullSpeed = 4f;
        [SerializeField, Min(0.1f), Tooltip("Clear gap (units) between the two bay rows. Copied from LotConfig's aisle width by the editor setup; a mismatch is reported at start.")]
        private float _measuredAisleWidth = 4.6f;
        [SerializeField, Range(0f, 3f), Tooltip("Units added to half the aisle to get R_min.")]
        private float _radiusMargin = 1f;
        [SerializeField, Range(0f, 1f), Tooltip("Units of space kept between a reversing car and the opposite bay row.")]
        private float _pullOutClearance = 0.3f;

        [Header("Car-on-car bounce")]
        [SerializeField, Range(0f, 1f), Tooltip("Restitution between two cars: 0 = dead stop, 1 = perfect rubber. Walls and the valet are unchanged.")]
        private float _bounciness = 0.4f;
        [SerializeField, Min(0f), Tooltip("Approach speed (units/s) below which a touch neither bounces nor makes a sound.")]
        private float _minImpactSpeed = 0.5f;
        [SerializeField, Min(0.1f), Tooltip("Approach speed (units/s) at which the bump counts as a full-strength hit.")]
        private float _fullImpactSpeed = 6f;
        [SerializeField, Min(1f), Tooltip("A car with nobody in it counts this many times heavier in a bump (it is braked), so a driven car visibly bounces off it while the parked car only shuffles.")]
        private float _parkedMassScale = 8f;
        [SerializeField, Min(0f), Tooltip("Seconds before the same pair of cars can report another bump.")]
        private float _bumpPairInterval = 0.25f;

        /// <summary>Restitution between two cars, 0..1.</summary>
        public float Bounciness => _bounciness;
        /// <summary>Approach speed below which a car-on-car touch is ignored, units per second.</summary>
        public float MinImpactSpeed => _minImpactSpeed;
        /// <summary>Approach speed of a full-strength bump, units per second.</summary>
        public float FullImpactSpeed => _fullImpactSpeed;
        /// <summary>How many times heavier a car without a driver counts in a bump.</summary>
        public float ParkedMassScale => _parkedMassScale;
        /// <summary>Minimum seconds between two bump reports of the same pair.</summary>
        public float BumpPairInterval => _bumpPairInterval;
        /// <summary>Sideways decay rate at full grip, per second.</summary>
        public float LateralFriction => _lateralFriction;
        /// <summary>Share of removed sideways speed handed back forward.</summary>
        public float ForwardGrip => _forwardGrip;
        /// <summary>Speed up to which grip is full, units per second.</summary>
        public float GripLowSpeed => _gripLowSpeed;
        /// <summary>Speed from which grip is <see cref="GripAtSpeed"/>, units per second.</summary>
        public float GripHighSpeed => _gripHighSpeed;
        /// <summary>Grip fraction at high speed.</summary>
        public float GripAtSpeed => _gripAtSpeed;
        /// <summary>Grip multiplier while the drift button is held.</summary>
        public float DriftGripScale => _driftGripScale;
        /// <summary>Speed above which the drift button has an effect.</summary>
        public float DriftMinSpeed => _driftMinSpeed;
        /// <summary>Speed at which steering reaches full effect, units per second.</summary>
        public float TurnFullSpeed => _turnFullSpeed;
        /// <summary>Measured gap between the bay rows, units.</summary>
        public float MeasuredAisleWidth => _measuredAisleWidth;
        /// <summary>Clearance kept when pulling out, units.</summary>
        public float PullOutClearance => _pullOutClearance;
        /// <summary>Tightest radius at low speed, units (derived from the aisle).</summary>
        public float RMin => TurningRadius.RMin(_measuredAisleWidth, _radiusMargin);
        /// <summary>Turn rate at full steering and full speed, radians per second (derived).</summary>
        public float MaxTurnRate => TurningRadius.MaxTurnRate(_turnFullSpeed, RMin);
    }
}
