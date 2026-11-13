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

        [Tooltip("Impatient patrons wait this fraction of the pickup window (1 = same as everyone).")]
        [SerializeField, Range(0.3f, 1f)] private float _impatientWindowScale = 0.6f;

        [Header("Stay start")]
        [SerializeField, Min(0.5f), Tooltip("Game minutes a car may sit in the arrival queue before the patron's stay starts anyway. Normally the stay starts when the car first rests clear of the queue.")]
        private float _queueWaitMinutes = ServiceTuning.DefaultQueueWaitMinutes;

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

        public float CurbWalkTimeoutMinutes => _curbWalkTimeoutMinutes;
        public float LateStayLeadMinutes => _lateStayLeadMinutes;
        public float LateStayFloorMinutes => _lateStayFloorMinutes;
        public float BoardingTimeoutMinutes => _boardingTimeoutMinutes;
        public float QueueWaitMinutes => _queueWaitMinutes;
        public ServiceTuning Tuning => new ServiceTuning(_queueWaitMinutes);
        public int MaxWaitingCars => _maxWaitingCars;
        public Vector2 ArrivalIntervalRange => _arrivalIntervalRange;
        public Vector2 StayRange => _stayRange;
        public int BaseTip => _baseTip;
        public float ImpatientChance => _impatientChance;
        public float PickupWindowMinutes => _pickupWindowMinutes;
        public float ImpatientWindowScale => _impatientWindowScale;
    }
}
