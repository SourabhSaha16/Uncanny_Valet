using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>How the director moves patron returns (spec section 5) and the late-shift pickups (issue 73).</summary>
    [Serializable]
    public sealed class ReturnSettings
    {
        [SerializeField, Range(0f, 50f), Tooltip("The director may move a return this many percent of the planned stay either way, before the late-shift phase.")]
        private float _windowPercent = 25f;
        [SerializeField, Min(0f), Tooltip("Fewest seconds between two patrons reaching the curb, except in a wave peak.")]
        private float _spacingSeconds = 4f;
        [SerializeField, Min(0f), Tooltip("From this many seconds before the end every open ticket's return is brought forward, outside the window. 0 = never.")]
        private float _lateStartSeconds = 60f;
        [SerializeField, Min(0f), Tooltip("The last late-shift pickup is requested this many seconds before the end.")]
        private float _lastLeadSeconds = 20f;
        [SerializeField, Min(0.5f), Tooltip("A return is never moved closer than this to now, so the move lands before the patron leaves.")]
        private float _minLeadSeconds = 3f;
        [SerializeField, Min(0.1f), Tooltip("Moves smaller than this many seconds are ignored.")]
        private float _minMoveSeconds = 0.25f;
        [SerializeField, Min(0f), Tooltip("Seconds between two bring-forward or hold-back nudges outside the late-shift phase.")]
        private float _cooldownSeconds = 10f;

        public float WindowFraction => _windowPercent * 0.01f;
        public float SpacingSeconds => _spacingSeconds;
        public float LateStartSeconds => _lateStartSeconds;
        public float LastLeadSeconds => _lastLeadSeconds;
        public float MinLeadSeconds => _minLeadSeconds;
        public float MinMoveSeconds => _minMoveSeconds;
        public float CooldownSeconds => _cooldownSeconds;

        /// <summary>Writes the starting values into the live tuning (Bootstrap only).</summary>
        public void Seed(ITuning t)
        {
            t.Seed(TuningKey.ReturnWindowPercent, _windowPercent);
            t.Seed(TuningKey.PickupSpacingSeconds, _spacingSeconds);
            t.Seed(TuningKey.LatePickupStart, _lateStartSeconds);
            t.Seed(TuningKey.LastPickupLead, _lastLeadSeconds);
        }

        /// <summary>Refreshes this runtime copy from the live tuning.</summary>
        public void Read(ITuning t)
        {
            _windowPercent = TuningRead.Float(t, TuningKey.ReturnWindowPercent, _windowPercent);
            _spacingSeconds = TuningRead.Float(t, TuningKey.PickupSpacingSeconds, _spacingSeconds);
            _lateStartSeconds = TuningRead.Float(t, TuningKey.LatePickupStart, _lateStartSeconds);
            _lastLeadSeconds = TuningRead.Float(t, TuningKey.LastPickupLead, _lastLeadSeconds);
        }
    }
}
