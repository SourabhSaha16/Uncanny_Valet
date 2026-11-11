using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>How the director turns the gap between workload and target into an arrival countdown (spec section 3).</summary>
    [Serializable]
    public sealed class PacingSettings
    {
        [SerializeField, Range(0.02f, 0.6f), Tooltip("Half-width of the on-target band around the target.")]
        private float _band = 0.15f;
        [SerializeField, Range(0f, 0.5f), Tooltip("How far form F (-1..1) moves the target.")]
        private float _formInfluence = 0.15f;
        [SerializeField, Range(0f, 1f), Tooltip("Workload below this counts as idle.")]
        private float _idleWorkload = 0.3f;
        [SerializeField, Min(1f), Tooltip("Seconds of idle after which a car is sent at once.")]
        private float _idleSeconds = 5f;
        [SerializeField, Range(0.2f, 1f), Tooltip("Interval multiplier when the player is under target.")]
        private float _underFactor = 0.6f;
        [SerializeField, Range(1f, 3f), Tooltip("Interval multiplier when the player is over target.")]
        private float _overFactor = 1.5f;
        [SerializeField, Min(0.05f), Tooltip("How fast the interval multiplier moves toward its goal, per second. Keeps re-scaling smooth.")]
        private float _factorSlewPerSecond = 0.5f;
        [SerializeField, Range(0f, 1f), Tooltip("How strongly the arrival budget pulls the interval toward an even spread (0 = not at all).")]
        private float _budgetBlend = 0.5f;
        [SerializeField, Min(1f), Tooltip("The shortest countdown between two arrivals, in seconds.")]
        private float _minIntervalSeconds = 5f;
        [SerializeField, Min(1f), Tooltip("The longest countdown between two arrivals, in seconds.")]
        private float _maxIntervalSeconds = 40f;
        [SerializeField, Min(1f), Tooltip("Seconds the pacer waits for a sent car to really arrive before it counts the send as lost and sends again.")]
        private float _spawnAckSeconds = 8f;

        [SerializeField, Range(0.5f, 30f), Tooltip("Longest the arrival queue may stay empty, in seconds, before a car is sent whatever the pacing says.")]
        private float _maxEmptyQueueSeconds = 2f;
        [SerializeField, Min(0f), Tooltip("No car is forced in when fewer seconds than this are left in the shift.")]
        private float _idleCutoffSeconds = 20f;

        // Not serialized: the lot's DirectorProfile owns the budget and seeds it (see Seed).
        private int _arrivalBudget = DirectorProfile.DefaultArrivalBudget;
        private int _budgetSlack = DirectorProfile.DefaultBudgetSlack;

        public float Band => _band;
        public float FormInfluence => _formInfluence;
        public float IdleWorkload => _idleWorkload;
        public float IdleSeconds => _idleSeconds;
        public float UnderFactor => _underFactor;
        public float OverFactor => _overFactor;
        public float FactorSlewPerSecond => _factorSlewPerSecond;
        public float BudgetBlend => _budgetBlend;
        public float MinIntervalSeconds => _minIntervalSeconds;
        public float MaxIntervalSeconds => _maxIntervalSeconds;
        public float SpawnAckSeconds => _spawnAckSeconds;
        public float MaxEmptyQueueSeconds => _maxEmptyQueueSeconds;
        public float IdleCutoffSeconds => _idleCutoffSeconds;
        public int ArrivalBudget => _arrivalBudget;
        public int BudgetSlack => _budgetSlack;

        /// <summary>The hard cap on arrivals in one shift: budget plus slack.</summary>
        public int MaxArrivals => _arrivalBudget + _budgetSlack;

        /// <summary>Writes the starting values into the live tuning (Bootstrap only). The budget comes from the profile.</summary>
        public void Seed(ITuning t, int arrivalBudget, int budgetSlack)
        {
            t.Seed(TuningKey.DirBand, _band);
            t.Seed(TuningKey.DirFormInfluence, _formInfluence);
            t.Seed(TuningKey.DirIdleWorkload, _idleWorkload);
            t.Seed(TuningKey.DirIdleSeconds, _idleSeconds);
            t.Seed(TuningKey.DirUnderFactor, _underFactor);
            t.Seed(TuningKey.DirOverFactor, _overFactor);
            t.Seed(TuningKey.DirFactorSlew, _factorSlewPerSecond);
            t.Seed(TuningKey.DirBudgetBlend, _budgetBlend);
            t.Seed(TuningKey.DirMinInterval, _minIntervalSeconds);
            t.Seed(TuningKey.DirMaxInterval, _maxIntervalSeconds);
            t.Seed(TuningKey.ArrivalIdleLimit, _maxEmptyQueueSeconds);
            t.Seed(TuningKey.ArrivalIdleCutoff, _idleCutoffSeconds);
            t.Seed(TuningKey.DirArrivalBudget, arrivalBudget);
            t.Seed(TuningKey.DirBudgetSlack, budgetSlack);
        }

        /// <summary>Refreshes this runtime copy from the live tuning.</summary>
        public void Read(ITuning t)
        {
            _band = TuningRead.Float(t, TuningKey.DirBand, _band);
            _formInfluence = TuningRead.Float(t, TuningKey.DirFormInfluence, _formInfluence);
            _idleWorkload = TuningRead.Float(t, TuningKey.DirIdleWorkload, _idleWorkload);
            _idleSeconds = TuningRead.Float(t, TuningKey.DirIdleSeconds, _idleSeconds);
            _underFactor = TuningRead.Float(t, TuningKey.DirUnderFactor, _underFactor);
            _overFactor = TuningRead.Float(t, TuningKey.DirOverFactor, _overFactor);
            _factorSlewPerSecond = TuningRead.Float(t, TuningKey.DirFactorSlew, _factorSlewPerSecond);
            _budgetBlend = TuningRead.Float(t, TuningKey.DirBudgetBlend, _budgetBlend);
            _minIntervalSeconds = TuningRead.Float(t, TuningKey.DirMinInterval, _minIntervalSeconds);
            _maxIntervalSeconds = Mathf.Max(_minIntervalSeconds,
                TuningRead.Float(t, TuningKey.DirMaxInterval, _maxIntervalSeconds));
            _maxEmptyQueueSeconds = TuningRead.Float(t, TuningKey.ArrivalIdleLimit, _maxEmptyQueueSeconds);
            _idleCutoffSeconds = TuningRead.Float(t, TuningKey.ArrivalIdleCutoff, _idleCutoffSeconds);
            _arrivalBudget = Mathf.RoundToInt(TuningRead.Float(t, TuningKey.DirArrivalBudget, _arrivalBudget));
            _budgetSlack = Mathf.RoundToInt(TuningRead.Float(t, TuningKey.DirBudgetSlack, _budgetSlack));
        }
    }
}
