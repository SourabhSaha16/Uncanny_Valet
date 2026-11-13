using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>How the archetype picker scales its base weights and limits its draws (spec sections 4 and 5).</summary>
    [Serializable]
    public sealed class PickerSettings
    {
        [SerializeField, Min(0.1f), Tooltip("Under target: Muddy and Muddy EV weights are multiplied by this.")]
        private float _underHeavy = 1.5f;
        [SerializeField, Min(0.1f), Tooltip("Over target: Muddy and Muddy EV weights are multiplied by this.")]
        private float _overHeavy = 0.5f;
        [SerializeField, Min(0.1f), Tooltip("Over target: the Plain weight is multiplied by this.")]
        private float _overPlain = 1.5f;
        [SerializeField, Min(1), Tooltip("At most this many arrivals in a row carrying the same service modifier (Muddy, or needing a charge).")]
        private int _streakLimit = 2;
        [SerializeField, Range(0.02f, 0.5f), Tooltip("Each archetype's share of the shift stays within this of its base share (0.1 = 10 points).")]
        private float _mixTolerance = 0.1f;
        [SerializeField, Tooltip("Assist hook: clamps form F at 0 and weakens every dilemma boost.")]
        private bool _assist;
        [SerializeField, Range(0f, 1f), Tooltip("In assist mode a dilemma boost keeps this fraction of its strength (0.5 = halved).")]
        private float _assistDilemmaScale = 0.5f;

        public float UnderHeavy => _underHeavy;
        public float OverHeavy => _overHeavy;
        public float OverPlain => _overPlain;
        public int StreakLimit => _streakLimit;
        public float MixTolerance => _mixTolerance;
        public bool Assist => _assist;
        public float AssistDilemmaScale => _assistDilemmaScale;

        /// <summary>Writes the starting values into the live tuning (Bootstrap only).</summary>
        public void Seed(ITuning t)
        {
            t.Seed(TuningKey.DirLoadUnderHeavy, _underHeavy);
            t.Seed(TuningKey.DirLoadOverHeavy, _overHeavy);
            t.Seed(TuningKey.DirLoadOverPlain, _overPlain);
            t.Seed(TuningKey.DirStreakLimit, _streakLimit);
            t.Seed(TuningKey.DirMixTolerance, _mixTolerance);
            t.Seed(TuningKey.DirAssist, _assist ? 1f : 0f);
            t.Seed(TuningKey.DirAssistDilemmaScale, _assistDilemmaScale);
        }

        /// <summary>Refreshes this runtime copy from the live tuning.</summary>
        public void Read(ITuning t)
        {
            _underHeavy = TuningRead.Float(t, TuningKey.DirLoadUnderHeavy, _underHeavy);
            _overHeavy = TuningRead.Float(t, TuningKey.DirLoadOverHeavy, _overHeavy);
            _overPlain = TuningRead.Float(t, TuningKey.DirLoadOverPlain, _overPlain);
            _streakLimit = Mathf.Max(1, Mathf.RoundToInt(TuningRead.Float(t, TuningKey.DirStreakLimit, _streakLimit)));
            _mixTolerance = TuningRead.Float(t, TuningKey.DirMixTolerance, _mixTolerance);
            _assist = TuningRead.Float(t, TuningKey.DirAssist, _assist ? 1f : 0f) >= 0.5f;
            _assistDilemmaScale = TuningRead.Float(t, TuningKey.DirAssistDilemmaScale, _assistDilemmaScale);
        }
    }
}
