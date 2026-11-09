using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>
    /// Every tunable number of the Patron Director that is not lot-specific: task costs, thresholds, cooldowns and
    /// the Enabled flag. Read-only at runtime: Bootstrap copies it into ITuning, and the F2 panel edits that copy.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Director Config", fileName = "DirectorConfig")]
    public sealed class DirectorConfig : ScriptableObject
    {
        [SerializeField, Tooltip("On: the director paces arrivals. Off: the old fixed 15-25 s timer, so a play-test can compare.")]
        private bool _enabled = true;
        [SerializeField, Range(0.1f, 2f), Tooltip("Seconds of game time between director ticks.")]
        private float _tickSeconds = 0.5f;
        [SerializeField, Min(0), Tooltip("0 picks a fresh random seed every shift. Otherwise the seed plus the shift number, so a shift can be replayed.")]
        private int _seed;

        [Header("Workload cost table")]
        [SerializeField] private WorkloadSettings _workload = new WorkloadSettings();
        [Header("Pacing")]
        [SerializeField] private PacingSettings _pacing = new PacingSettings();
        [Header("Relief")]
        [SerializeField] private ReliefSettings _relief = new ReliefSettings();
        [Header("Form")]
        [SerializeField] private FormSettings _form = new FormSettings();

        public bool Enabled => _enabled;

        /// <summary>Copies every number into the live tuning. The profile supplies the lot's arrival budget.</summary>
        public void SeedInto(ITuning tuning, DirectorProfile profile)
        {
            tuning.Seed(TuningKey.DirEnabled, _enabled ? 1f : 0f);
            tuning.Seed(TuningKey.DirTickSeconds, _tickSeconds);
            tuning.Seed(TuningKey.DirSeed, _seed);
            _workload.Seed(tuning);
            _pacing.Seed(tuning, profile.ArrivalBudget, profile.BudgetSlack);
            _relief.Seed(tuning);
            _form.Seed(tuning);
        }
    }
}
