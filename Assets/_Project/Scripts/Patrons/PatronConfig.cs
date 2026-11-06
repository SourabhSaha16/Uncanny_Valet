using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>Tunable patron numbers (design.md sections 5 and 10). Times are game minutes.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Patron Config", fileName = "PatronConfig")]
    public class PatronConfig : ScriptableObject
    {
        [Tooltip("Game minutes between arrivals (min, max).")]
        [SerializeField] private Vector2 _arrivalIntervalRange = new Vector2(8f, 15f);

        [Tooltip("Game minutes a car stays parked before pickup is requested (min, max).")]
        [SerializeField] private Vector2 _stayRange = new Vector2(20f, 45f);

        [SerializeField, Min(0)] private int _baseTip = 20;
        [SerializeField, Range(0f, 1f)] private float _impatientChance = 0.25f;

        [Tooltip("Cars awaiting parking at once. Parked cars do not count.")]
        [SerializeField, Min(1)] private int _maxWaitingCars = 3;

        [Tooltip("Game minutes to get a waiting car parked before the patron takes it and leaves.")]
        [SerializeField, Min(1f)] private float _parkWindowMinutes = PatronRequest.DefaultParkWindowMinutes;

        [Tooltip("Game minutes to bring a requested car to the curb before the tip drops to $0.")]
        [SerializeField, Min(1f)] private float _pickupWindowMinutes = PatronRequest.DefaultPickupWindowMinutes;

        public int MaxWaitingCars => _maxWaitingCars;
        public Vector2 ArrivalIntervalRange => _arrivalIntervalRange;
        public Vector2 StayRange => _stayRange;
        public int BaseTip => _baseTip;
        public float ImpatientChance => _impatientChance;
        public float ParkWindowMinutes => _parkWindowMinutes;
        public float PickupWindowMinutes => _pickupWindowMinutes;
    }
}
