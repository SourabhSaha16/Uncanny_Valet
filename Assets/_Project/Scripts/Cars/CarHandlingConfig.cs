using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Handling numbers of docs/redesign/car_handling_spec.md section 3 (turning radius from the real aisle). Read only
    /// at runtime. Not wired to any car yet; the drift and steering models of rollout step 2 read it.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Car Handling Config", fileName = "CarHandlingConfig")]
    public sealed class CarHandlingConfig : ScriptableObject
    {
        [SerializeField, Min(0.1f), Tooltip("Speed (units/s) at and above which steering has full effect; below it the turn rate scales down.")]
        private float _turnFullSpeed = 4f;
        [SerializeField, Min(0.1f), Tooltip("Clear gap (units) between the two bay rows, measured from LotGeometry. Must follow LotConfig's aisle width.")]
        private float _measuredAisleWidth = 4.6f;
        [SerializeField, Range(0f, 3f), Tooltip("Units added to half the aisle to get R_min.")]
        private float _radiusMargin = 1f;
        [SerializeField, Range(0f, 1f), Tooltip("Units of space kept between a reversing car and the opposite bay row.")]
        private float _pullOutClearance = 0.3f;

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
