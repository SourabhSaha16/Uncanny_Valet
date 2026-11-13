using System;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>The position one dial starts at in a fresh game.</summary>
    [Serializable]
    public struct DialStart
    {
        [SerializeField] private TuningKey _dial;
        [SerializeField, Min(0f)] private float _position;

        public DialStart(TuningKey dial, float position) { _dial = dial; _position = position; }

        public TuningKey Dial => _dial;
        public float Position => _position;

        /// <summary>The designer baseline of 2026-11-10 (tuned by hand in the F2 panel).</summary>
        public static System.Collections.Generic.List<DialStart> Baseline() => new System.Collections.Generic.List<DialStart>
        {
            new DialStart(TuningKey.DialIntensity, 8f),
            new DialStart(TuningKey.DialRamp, 8f),
            new DialStart(TuningKey.DialLateRush, 6f),
            new DialStart(TuningKey.DialDilemmas, 6.5f),
            new DialStart(TuningKey.DialMercy, 4f),
            new DialStart(TuningKey.DialSkillReward, 5.5f),
        };
    }
}
