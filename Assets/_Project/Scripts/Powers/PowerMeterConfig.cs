using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Powers
{
    /// <summary>Numbers of the power meter and the timed powers (docs/redesign/valet_powers.md). Read-only at runtime.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Power Meter Config", fileName = "PowerMeterConfig")]
    public sealed class PowerMeterConfig : ScriptableObject
    {
        [Header("Meter fill (meter runs 0 to the level 3 threshold)")]
        [SerializeField, Range(0f, 100f), Tooltip("Meter gained for one Perfect Park.")]
        private float _fillPerfectPark = 18f;

        [SerializeField, Range(0f, 100f), Tooltip("Meter gained for one perfect drop-off.")]
        private float _fillPerfectDropOff = 10f;

        [SerializeField, Range(0f, 5f), Tooltip("Meter gained per second of sliding.")]
        private float _fillDriftPerSecond = 0.6f;

        [SerializeField, Range(0f, 20f), Tooltip("Most meter one slide can add.")]
        private float _fillDriftCap = 4f;

        [Header("Levels")]
        [SerializeField, Min(1f), Tooltip("Meter value that unlocks level 1, the speed boost.")]
        private float _level1Threshold = 25f;

        [SerializeField, Min(1f), Tooltip("Meter value that unlocks level 2, freeze time.")]
        private float _level2Threshold = 55f;

        [SerializeField, Min(1f), Tooltip("Meter value that unlocks level 3, Park Smash. Also the full bar.")]
        private float _level3Threshold = 100f;

        [Header("Speed boost")]
        [SerializeField, Min(0.5f), Tooltip("Seconds the speed boost lasts.")]
        private float _speedBoostSeconds = 4f;

        [SerializeField, Range(1f, 3f), Tooltip("Walk speed multiplier during the boost.")]
        private float _walkMultiplier = 1.4f;

        [SerializeField, Range(0.1f, 2f), Tooltip("Seconds a pedestrian stumbles when the boosted valet runs through them.")]
        private float _stumbleSeconds = 0.4f;

        [SerializeField, Range(0f, 60f), Tooltip("Sparkles per second in the boosted valet's trail.")]
        private float _trailPerSecond = 24f;

        [Header("Freeze time")]
        [SerializeField, Min(0.5f), Tooltip("Seconds freeze time lasts.")]
        private float _freezeSeconds = 6f;

        [SerializeField, Range(0f, 1f), Tooltip("World time scale during freeze time: 0 = stopped, 0.3 = slow.")]
        private float _freezeTimeScale;

        public float FillPerfectPark => _fillPerfectPark;
        public float FillPerfectDropOff => _fillPerfectDropOff;
        public float FillDriftPerSecond => _fillDriftPerSecond;
        public float FillDriftCap => _fillDriftCap;
        public float Level1Threshold => _level1Threshold;
        public float Level2Threshold => _level2Threshold;
        public float Level3Threshold => _level3Threshold;
        public float SpeedBoostSeconds => _speedBoostSeconds;
        public float WalkMultiplier => _walkMultiplier;
        public float StumbleSeconds => _stumbleSeconds;
        public float TrailPerSecond => _trailPerSecond;
        public float FreezeSeconds => _freezeSeconds;
        public float FreezeTimeScale => _freezeTimeScale;

        /// <summary>Copies every number into the tuning store (Bootstrap calls this once at startup).</summary>
        public void SeedInto(ITuning t)
        {
            t.Seed(TuningKey.PowerFillPerfectPark, _fillPerfectPark);
            t.Seed(TuningKey.PowerFillPerfectDropOff, _fillPerfectDropOff);
            t.Seed(TuningKey.PowerFillDriftPerSecond, _fillDriftPerSecond);
            t.Seed(TuningKey.PowerFillDriftCap, _fillDriftCap);
            t.Seed(TuningKey.PowerLevel1Threshold, _level1Threshold);
            t.Seed(TuningKey.PowerLevel2Threshold, _level2Threshold);
            t.Seed(TuningKey.PowerLevel3Threshold, _level3Threshold);
            t.Seed(TuningKey.PowerSpeedBoostSeconds, _speedBoostSeconds);
            t.Seed(TuningKey.PowerWalkMultiplier, _walkMultiplier);
            t.Seed(TuningKey.PowerStumbleSeconds, _stumbleSeconds);
            t.Seed(TuningKey.PowerTrailPerSecond, _trailPerSecond);
            t.Seed(TuningKey.PowerFreezeSeconds, _freezeSeconds);
            t.Seed(TuningKey.PowerFreezeTimeScale, _freezeTimeScale);
        }

        private void OnValidate()
        {
            _level2Threshold = Mathf.Max(_level2Threshold, _level1Threshold);
            _level3Threshold = Mathf.Max(_level3Threshold, _level2Threshold);
        }
    }
}
