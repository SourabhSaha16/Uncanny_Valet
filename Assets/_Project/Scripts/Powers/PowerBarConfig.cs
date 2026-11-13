using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Powers
{
    /// <summary>Numbers of the shared power bar and the timed powers. Read-only at runtime.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Power Bar Config", fileName = "PowerBarConfig")]
    public sealed class PowerBarConfig : ScriptableObject
    {
        [Header("Power bar fill (the bar runs 0 to 100; a third of it is one bucket)")]
        [SerializeField, Range(0f, 100f), Tooltip("Bar gained for one Perfect Park.")]
        private float _fillPerfectPark = 18f;

        [SerializeField, Range(0f, 100f), Tooltip("Bar gained for one perfect drop-off.")]
        private float _fillPerfectDropOff = 10f;

        [SerializeField, Range(0f, 5f), Tooltip("Bar gained per second of sliding.")]
        private float _fillDriftPerSecond = 0.6f;

        [SerializeField, Range(0f, 20f), Tooltip("Most bar one slide can add.")]
        private float _fillDriftCap = 4f;

        [Header("Freeze time")]
        [SerializeField, Min(0.5f), Tooltip("Seconds freeze time lasts.")]
        private float _freezeSeconds = 6f;

        [SerializeField, Range(0f, 1f), Tooltip("World time scale during freeze time: 0 = stopped, 0.3 = slow.")]
        private float _freezeTimeScale;

        public float FillPerfectPark => _fillPerfectPark;
        public float FillPerfectDropOff => _fillPerfectDropOff;
        public float FillDriftPerSecond => _fillDriftPerSecond;
        public float FillDriftCap => _fillDriftCap;
        public float FreezeSeconds => _freezeSeconds;
        public float FreezeTimeScale => _freezeTimeScale;

        /// <summary>Copies every number into the tuning store (Bootstrap calls this once at startup).</summary>
        public void SeedInto(ITuning t)
        {
            t.Seed(TuningKey.PowerFillPerfectPark, _fillPerfectPark);
            t.Seed(TuningKey.PowerFillPerfectDropOff, _fillPerfectDropOff);
            t.Seed(TuningKey.PowerFillDriftPerSecond, _fillDriftPerSecond);
            t.Seed(TuningKey.PowerFillDriftCap, _fillDriftCap);
            t.Seed(TuningKey.PowerFreezeSeconds, _freezeSeconds);
            t.Seed(TuningKey.PowerFreezeTimeScale, _freezeTimeScale);
            t.Seed(TuningKey.DevOwnFreezeTime, 0f);
        }
    }
}
