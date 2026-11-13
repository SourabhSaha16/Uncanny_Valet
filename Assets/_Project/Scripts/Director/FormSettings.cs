using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace UncannyValet.Director
{
    /// <summary>How form F (how well the player is doing) is measured and smoothed (spec section 2).</summary>
    [Serializable]
    public sealed class FormSettings
    {
        [SerializeField, Min(1f), Tooltip("Seconds between form updates.")]
        private float _updateSeconds = 15f;
        [SerializeField, Range(0.05f, 1f), Tooltip("The most form can move in one update.")]
        private float _maxStep = 0.25f;
        [SerializeField, Min(10f), Tooltip("Seconds of recent blessings and angry patrons that count.")]
        private float _windowSeconds = 60f;
        [SerializeField, Min(0f), Tooltip("Seconds at the start of a shift during which form stays 0 (no blessings can exist yet).")]
        private float _warmupSeconds = 30f;
        [SerializeField, Min(0f), Tooltip("Weight of the blessing rate against the pace target (-1 well behind, +1 well ahead).")]
        [FormerlySerializedAs("_tipWeight")] private float _blessingWeight = 0.6f;
        [SerializeField, Min(0f), Tooltip("Weight of recent angry patrons (pulls form down).")]
        private float _angryWeight = 1f;
        [SerializeField, Min(0f), Tooltip("Weight of the current favour (pushes form up).")]
        [FormerlySerializedAs("_comboWeight")] private float _favourWeight = 0.4f;
        [SerializeField, Min(0), Tooltip("Blessings per shift that count as exactly on pace (the blessing term of form is 0 at this rate). Was the old three-star score.")]
        private int _paceBlessings = 450;
        [SerializeField, Min(1), Tooltip("Angry patrons in the window that give the full angry penalty.")]
        private int _angryForFullPenalty = 2;

        public float UpdateSeconds => _updateSeconds;
        public float MaxStep => _maxStep;
        public float WindowSeconds => _windowSeconds;
        public float WarmupSeconds => _warmupSeconds;
        public float BlessingWeight => _blessingWeight;
        public float AngryWeight => _angryWeight;
        public float FavourWeight => _favourWeight;
        public int PaceBlessings => _paceBlessings;
        public int AngryForFullPenalty => _angryForFullPenalty;

        /// <summary>Writes the starting values into the live tuning (Bootstrap only).</summary>
        public void Seed(ITuning t)
        {
            t.Seed(TuningKey.DirFormUpdate, _updateSeconds);
            t.Seed(TuningKey.DirFormMaxStep, _maxStep);
            t.Seed(TuningKey.DirFormWindow, _windowSeconds);
            t.Seed(TuningKey.DirFormWarmup, _warmupSeconds);
            t.Seed(TuningKey.DirFormBlessingWeight, _blessingWeight);
            t.Seed(TuningKey.DirFormAngryWeight, _angryWeight);
            t.Seed(TuningKey.DirFormFavourWeight, _favourWeight);
            t.Seed(TuningKey.DirFormAngryFull, _angryForFullPenalty);
        }

        /// <summary>Refreshes this runtime copy from the live tuning.</summary>
        public void Read(ITuning t)
        {
            _updateSeconds = Mathf.Max(1f, TuningRead.Float(t, TuningKey.DirFormUpdate, _updateSeconds));
            _maxStep = TuningRead.Float(t, TuningKey.DirFormMaxStep, _maxStep);
            _windowSeconds = Mathf.Max(1f, TuningRead.Float(t, TuningKey.DirFormWindow, _windowSeconds));
            _warmupSeconds = TuningRead.Float(t, TuningKey.DirFormWarmup, _warmupSeconds);
            _blessingWeight = TuningRead.Float(t, TuningKey.DirFormBlessingWeight, _blessingWeight);
            _angryWeight = TuningRead.Float(t, TuningKey.DirFormAngryWeight, _angryWeight);
            _favourWeight = TuningRead.Float(t, TuningKey.DirFormFavourWeight, _favourWeight);
            _angryForFullPenalty = Mathf.Max(1,
                Mathf.RoundToInt(TuningRead.Float(t, TuningKey.DirFormAngryFull, _angryForFullPenalty)));
        }
    }
}
