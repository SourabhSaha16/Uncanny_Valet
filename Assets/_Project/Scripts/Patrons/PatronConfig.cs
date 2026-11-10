using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>Tunable patron numbers (design.md sections 5 and 10). Times are game minutes.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Patron Config", fileName = "PatronConfig")]
    public class PatronConfig : ScriptableObject
    {
        [Tooltip("Game minutes between arrivals (min, max). With the default clock one game minute is one second, so 15-25 s.")]
        [SerializeField] private Vector2 _arrivalIntervalRange = new Vector2(15f, 25f);

        [Tooltip("Game minutes a car stays parked before pickup is requested (min, max).")]
        [SerializeField] private Vector2 _stayRange = new Vector2(20f, 45f);

        [SerializeField, Min(0)] private int _baseTip = 20;
        [SerializeField, Range(0f, 1f)] private float _impatientChance = 0.25f;

        [Tooltip("Cars waiting at the arrival slots at once. Parked cars do not count.")]
        [SerializeField, Range(1, 3)] private int _maxWaitingCars = 3;

        [Tooltip("Game minutes to bring a requested car to the curb before the tip drops to $0.")]
        [SerializeField, Min(1f)] private float _pickupWindowMinutes = PatronRequest.DefaultPickupWindowMinutes;

        [Tooltip("Charge an EV arrives with (0..1).")]
        [SerializeField, Range(0f, 1f)] private float _evStartCharge = 0.2f;

        [Tooltip("Game minutes in a charging bay to go from empty to full.")]
        [SerializeField, Min(1f)] private float _chargeMinutes = 20f;

        [Tooltip("Game minutes in the wash bay to get a dirty car clean.")]
        [SerializeField, Min(1f)] private float _washMinutes = 15f;

        [Header("Walking patrons")]
        [SerializeField, Min(0f), Tooltip("Game minutes a patron may take to walk from the diner to the curb before the pickup is requested anyway. 0 requests the pickup the moment the stay ends (no walking).")]
        private float _curbWalkTimeoutMinutes = 40f;
        [SerializeField, Min(0f), Tooltip("Game minutes a delivered car waits for its patron to get in before driving off anyway. 0 sends it off at once.")]
        private float _boardingTimeoutMinutes = 6f;

        public float CurbWalkTimeoutMinutes => _curbWalkTimeoutMinutes;
        public float BoardingTimeoutMinutes => _boardingTimeoutMinutes;
        public ServiceTuning Tuning => new ServiceTuning(_evStartCharge, _chargeMinutes, _washMinutes);
        public int MaxWaitingCars => _maxWaitingCars;
        public Vector2 ArrivalIntervalRange => _arrivalIntervalRange;
        public Vector2 StayRange => _stayRange;
        public int BaseTip => _baseTip;
        public float ImpatientChance => _impatientChance;
        public float PickupWindowMinutes => _pickupWindowMinutes;
        public float ChargeMinutes => _chargeMinutes;
        public float WashMinutes => _washMinutes;
    }
}
