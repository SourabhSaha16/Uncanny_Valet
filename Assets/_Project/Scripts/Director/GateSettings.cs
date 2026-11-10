using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>The dilemma budget: when a dilemma may be made at all (spec section 4, "Dilemma budget").</summary>
    [Serializable]
    public sealed class GateSettings
    {
        [SerializeField, Min(0f), Tooltip("Seconds after a dilemma resolves before the next one may be made.")]
        private float _cooldownSeconds = 25f;
        [SerializeField, Min(0f), Tooltip("Seconds after an angry patron with no new dilemma (no double punishment).")]
        private float _blackoutSeconds = 20f;
        [SerializeField, Range(0f, 1f), Tooltip("A dilemma needs headroom: the pacing gap at or below this, or the curve rising.")]
        private float _headroomGap = 0.15f;

        public float CooldownSeconds => _cooldownSeconds;
        public float BlackoutSeconds => _blackoutSeconds;
        public float HeadroomGap => _headroomGap;

        /// <summary>Writes the starting values into the live tuning (Bootstrap only).</summary>
        public void Seed(ITuning t)
        {
            t.Seed(TuningKey.DirDilemmaCooldown, _cooldownSeconds);
            t.Seed(TuningKey.DirDilemmaBlackout, _blackoutSeconds);
            t.Seed(TuningKey.DirHeadroomGap, _headroomGap);
        }

        /// <summary>Refreshes this runtime copy from the live tuning.</summary>
        public void Read(ITuning t)
        {
            _cooldownSeconds = TuningRead.Float(t, TuningKey.DirDilemmaCooldown, _cooldownSeconds);
            _blackoutSeconds = TuningRead.Float(t, TuningKey.DirDilemmaBlackout, _blackoutSeconds);
            _headroomGap = TuningRead.Float(t, TuningKey.DirHeadroomGap, _headroomGap);
        }
    }
}
