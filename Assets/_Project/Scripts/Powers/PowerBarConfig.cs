using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
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
        [SerializeField, Range(0f, 1f), Tooltip("World time scale during freeze time: 0 = stopped, 0.3 = slow.")]
        private float _freezeTimeScale;

        [Header("Story powers")]
        [SerializeField, Tooltip("One definition per story power (names, icons, levels). A power without one uses the shipped defaults.")]
        private List<PowerDefinition> _definitions = new List<PowerDefinition>();

        public float FillPerfectPark => _fillPerfectPark;
        public float FillPerfectDropOff => _fillPerfectDropOff;
        public float FillDriftPerSecond => _fillDriftPerSecond;
        public float FillDriftCap => _fillDriftCap;
        public float FreezeTimeScale => _freezeTimeScale;

        /// <summary>The authored numbers of a power level: from its definition, else the shipped defaults.</summary>
        public PowerLevelValues ValuesFor(StoryPower power, int level)
        {
            for (var i = 0; i < _definitions.Count; i++)
                if (_definitions[i] != null && _definitions[i].Power == power && _definitions[i].TryGetValues(level, out var values)) return values;
            return PowerDefaults.Values(power, level);
        }

        /// <summary>Copies every number into the tuning store (Bootstrap calls this once at startup).</summary>
        public void SeedInto(ITuning t)
        {
            t.Seed(TuningKey.PowerFillPerfectPark, _fillPerfectPark);
            t.Seed(TuningKey.PowerFillPerfectDropOff, _fillPerfectDropOff);
            t.Seed(TuningKey.PowerFillDriftPerSecond, _fillDriftPerSecond);
            t.Seed(TuningKey.PowerFillDriftCap, _fillDriftCap);
            t.Seed(TuningKey.PowerFreezeTimeScale, _freezeTimeScale);
            SeedLevels(t);
            for (var i = 0; i < PowerLevelRows.All.Count; i++)
                if (PowerRows.TryGetDeveloperKey(PowerLevelRows.All[i], out var key)) t.Seed(key, 0f);
        }

        private void SeedLevels(ITuning t)
        {
            for (var i = 0; i < PowerLevelRows.All.Count; i++)
                for (var level = 1; level <= PowerTuningKeys.Levels; level++)
                {
                    var values = ValuesFor(PowerLevelRows.All[i], level);
                    for (var f = 0; f < PowerTuningKeys.FieldCount; f++)
                        if (PowerTuningKeys.TryGetKey(PowerLevelRows.All[i], level, (PowerField)f, out var key)) t.Seed(key, values.Get((PowerField)f));
                }        }
    }
}
