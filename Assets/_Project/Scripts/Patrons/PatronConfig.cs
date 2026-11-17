using UnityEngine;
using UnityEngine.Serialization;

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

        [SerializeField, Range(0f, 0.9f), Tooltip("Each patron's stay is moved by a random amount up to this fraction of itself, either way, so returns spread out instead of all coming at once. 0 turns it off.")]
        private float _stayJitterFraction = 0.3f;

        [SerializeField, Min(0)] [FormerlySerializedAs("_baseTip")] private int _baseBlessing = 20;
        [SerializeField, Range(0f, 1f)] private float _impatientChance = 0.25f;

        [Tooltip("The shift plan's starting cap on cars waiting to be parked, which the Patron Director reads. The queue itself holds up to Pickup.MaxQueuedCars.")]
        [SerializeField, Range(1, 4)] private int _maxWaitingCars = 3;

        [Tooltip("Game minutes to bring a requested car to the curb before the blessing drops to 0.")]
        [SerializeField, Min(1f)] private float _pickupWindowMinutes = PatronRequest.DefaultPickupWindowMinutes;

        [Tooltip("Impatient patrons wait this fraction of the pickup window (1 = same as everyone).")]
        [SerializeField, Range(0.3f, 1f)] private float _impatientWindowScale = 0.6f;

        [Header("Valet pickup spot and queue")]
        [SerializeField] private PickupQueueSettings _pickup = new PickupQueueSettings();

        [Tooltip("Crazy lots only: game minutes between two cars sent to keep the queue full, so arriving cars do not appear on top of each other. 0 sends every free slot at once. Training lots keep the director's pacing.")]
        [SerializeField, Min(0f)] private float _topUpSpacingMinutes = 1f;

        [Header("Late arrivals")]
        [SerializeField, Min(0f), Tooltip("Game minutes before the end of the shift by which a patron must be asking for the car, so every parked car gets its patron back. A stay that would run later is cut short.")]
        private float _lateStayLeadMinutes = 25f;
        [SerializeField, Min(1f), Tooltip("The shortest stay a late arrival is cut down to, in game minutes.")]
        private float _lateStayFloorMinutes = 8f;

        [Header("Walking patrons")]
        [SerializeField, Min(0f), Tooltip("Game minutes a patron may take to walk from the diner to the curb before the pickup is requested anyway. 0 requests the pickup the moment the stay ends (no walking).")]
        private float _curbWalkTimeoutMinutes = 40f;
        [SerializeField, Min(0f), Tooltip("Game minutes a delivered car waits for its patron to get in before driving off anyway. 0 sends it off at once.")]
        private float _boardingTimeoutMinutes = 6f;

        public float TopUpSpacingMinutes => _topUpSpacingMinutes;
        public float CurbWalkTimeoutMinutes => _curbWalkTimeoutMinutes;
        public float StayJitterFraction => _stayJitterFraction;
        public float LateStayLeadMinutes => _lateStayLeadMinutes;
        public float LateStayFloorMinutes => _lateStayFloorMinutes;
        public float BoardingTimeoutMinutes => _boardingTimeoutMinutes;
        public PickupQueueSettings Pickup => _pickup;
        public int MaxWaitingCars => _maxWaitingCars;
        public Vector2 ArrivalIntervalRange => _arrivalIntervalRange;
        public Vector2 StayRange => _stayRange;
        public int BaseBlessing => _baseBlessing;
        public float ImpatientChance => _impatientChance;
        public float PickupWindowMinutes => _pickupWindowMinutes;
        public float ImpatientWindowScale => _impatientWindowScale;
    }
}
