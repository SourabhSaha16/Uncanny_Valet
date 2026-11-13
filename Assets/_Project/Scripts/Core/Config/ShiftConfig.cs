using UncannyValet.Core.Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace UncannyValet.Core.Config
{
    /// <summary>
    /// Every tunable number of one shift: its length, the favour and curse rules, and
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

        [Header("Blessings")]
        [SerializeField, FormerlySerializedAs("_complaintPenalty"), Min(0), Tooltip("Blessings a curse takes off by default (an angry patron). A curse source may ask for another amount.")]
        private int _defaultCurse = 15;
        [SerializeField, FormerlySerializedAs("_maxComboMultiplier"), Range(1, 8), Tooltip("Highest favour multiplier. Each back-to-back happy patron raises it one step.")]
        private int _maxFavourMultiplier = 4;

        [Header("Pacing plan")]
        [SerializeField, Range(1, 3), Tooltip("Most cars waiting at the arrival slots at once (the lot has a column of 3 slots under the curb).")]
        private int _maxWaitingCars = 3;
        [SerializeField, Min(0.1f), Tooltip("Multiplies the gap between arrivals; below 1 is busier.")]
        private float _arrivalGapScale = 1f;
        [SerializeField, Min(0), Tooltip("Regular parking spots open, counted in grid order from the curb (the lot has 6; the EV bay and the wash slot are extra). 0 = all regular spots.")]
        private int _activeSpots;
        [SerializeField, Range(0f, 1f), Tooltip("Arrival chance of the late-ramping modifier (needs a charge) at the end of the shift.")]
        private float _lateEvChance = 0.5f;
        [SerializeField, Range(0f, 1f), Tooltip("Fraction of the shift after which the late-ramping chance moves up.")]
        private float _lateEvStart = 0.6f;
        [SerializeField] private bool _chargingOpen = true;
        [SerializeField] private bool _washOpen = true;

        public float ShiftLengthSeconds => _shiftLengthSeconds;
        public float GameMinutesPerSecond => _gameMinutesPerSecond;
        public float FinalStretchSeconds => _finalStretchSeconds;
        /// <summary>Blessings a curse takes off by default.</summary>
        public int DefaultCurse => _defaultCurse;
        public int MaxFavourMultiplier => _maxFavourMultiplier;

        /// <summary>The pacing rules for the shift.</summary>
        public ShiftPlan Plan => new ShiftPlan(_maxWaitingCars, _arrivalGapScale,
            _chargingOpen, _washOpen, _activeSpots, _lateEvChance, _lateEvStart);
    }
}
