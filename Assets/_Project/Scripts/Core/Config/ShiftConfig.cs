using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Core.Config
{
    /// <summary>
    /// Every tunable number of one shift: its length, the star thresholds, the combo and complaint rules, and
    /// the pacing plan. Read-only at runtime. Lives in Core because the Night, Economy and Bootstrap modules all read it.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Shift Config", fileName = "ShiftConfig")]
    public sealed class ShiftConfig : ScriptableObject
    {
        [Header("Clock")]
        [SerializeField, Min(30f), Tooltip("Real seconds in one shift. 240 = 4 minutes.")]
        private float _shiftLengthSeconds = 240f;
        [SerializeField, Min(0.01f), Tooltip("Game minutes that pass per real second. Patron timers count in game minutes; 1 makes a minute one second.")]
        private float _gameMinutesPerSecond = 1f;
        [SerializeField, Min(1f), Tooltip("The Last30Seconds event fires when this many seconds are left.")]
        private float _finalStretchSeconds = 30f;

        [Header("Score")]
        [SerializeField, Min(1), Tooltip("Score for one star. Placeholder.")]
        private int _oneStarScore = 150;
        [SerializeField, Min(1), Tooltip("Score for two stars. Placeholder.")]
        private int _twoStarScore = 300;
        [SerializeField, Min(1), Tooltip("Score for three stars. Placeholder.")]
        private int _threeStarScore = 450;
        [SerializeField, Min(0), Tooltip("Dollars taken off the score when a patron storms off angry.")]
        private int _complaintPenalty = 15;
        [SerializeField, Range(1, 8), Tooltip("Highest combo multiplier. Each back-to-back happy patron raises it one step.")]
        private int _maxComboMultiplier = 4;

        [Header("Pacing plan")]
        [SerializeField, Range(1, 4), Tooltip("Most cars waiting at the arrival slots at once (the lot has 4 side-by-side slots).")]
        private int _maxWaitingCars = 4;
        [SerializeField, Min(0.1f), Tooltip("Multiplies the gap between arrivals; below 1 is busier.")]
        private float _arrivalGapScale = 1f;
        [SerializeField, Min(0), Tooltip("Regular parking spots open, counted in grid order from the curb (the lot has 9; the EV bays and the wash slot are extra). 0 = all regular spots.")]
        private int _activeSpots;
        [SerializeField, Range(0f, 1f)] private float _evChance = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _dirtyChance = 0.3f;
        [SerializeField, Range(0f, 1f), Tooltip("EV chance reached at the end of the shift.")]
        private float _lateEvChance = 0.5f;
        [SerializeField, Range(0f, 1f), Tooltip("Fraction of the shift after which the EV chance ramps up.")]
        private float _lateEvStart = 0.6f;
        [SerializeField] private bool _chargingOpen = true;
        [SerializeField] private bool _washOpen = true;

        public float ShiftLengthSeconds => _shiftLengthSeconds;
        public float GameMinutesPerSecond => _gameMinutesPerSecond;
        public float FinalStretchSeconds => _finalStretchSeconds;
        public int OneStarScore => _oneStarScore;
        public int TwoStarScore => _twoStarScore;
        public int ThreeStarScore => _threeStarScore;
        public int ComplaintPenalty => _complaintPenalty;
        public int MaxComboMultiplier => _maxComboMultiplier;

        /// <summary>The pacing rules for the shift.</summary>
        public ShiftPlan Plan => new ShiftPlan(_maxWaitingCars, _arrivalGapScale, _evChance, _dirtyChance,
            _chargingOpen, _washOpen, _activeSpots, _lateEvChance, _lateEvStart);

        private void OnValidate()
        {
            if (_twoStarScore <= _oneStarScore) _twoStarScore = _oneStarScore + 1;
            if (_threeStarScore <= _twoStarScore) _threeStarScore = _twoStarScore + 1;
        }
    }
}
