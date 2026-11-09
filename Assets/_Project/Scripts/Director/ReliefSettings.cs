using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>When the player counts as drowning and what relief does about it (spec sections 3 and 7).</summary>
    [Serializable]
    public sealed class ReliefSettings
    {
        [SerializeField, Min(0.1f), Tooltip("Workload above this is drowning.")]
        private float _workload = 1.4f;
        [SerializeField, Range(0.1f, 1f), Tooltip("Pressure P (1 - the least patience left) above this is drowning.")]
        private float _pressure = 0.85f;
        [SerializeField, Min(1), Tooltip("This many angry patrons within the angry window is drowning.")]
        private int _angryCount = 2;
        [SerializeField, Min(1f), Tooltip("Seconds the angry patrons are counted over.")]
        private float _angryWindowSeconds = 30f;
        [SerializeField, Min(0f), Tooltip("Seconds all arrivals are held when relief starts.")]
        private float _holdSeconds = 10f;
        [SerializeField, Min(0), Tooltip("Plain cars sent after the hold, then relief ends.")]
        private int _plainCars = 2;
        [SerializeField, Min(0f), Tooltip("Seconds after relief ends before it can start again.")]
        private float _cooldownSeconds = 20f;

        public float Workload => _workload;
        public float Pressure => _pressure;
        public int AngryCount => _angryCount;
        public float AngryWindowSeconds => _angryWindowSeconds;
        public float HoldSeconds => _holdSeconds;
        public int PlainCars => _plainCars;
        public float CooldownSeconds => _cooldownSeconds;

        /// <summary>Writes the starting values into the live tuning (Bootstrap only).</summary>
        public void Seed(ITuning t)
        {
            t.Seed(TuningKey.DirReliefWorkload, _workload);
            t.Seed(TuningKey.DirReliefPressure, _pressure);
            t.Seed(TuningKey.DirReliefAngryCount, _angryCount);
            t.Seed(TuningKey.DirReliefAngryWindow, _angryWindowSeconds);
            t.Seed(TuningKey.DirReliefHold, _holdSeconds);
            t.Seed(TuningKey.DirReliefPlainCars, _plainCars);
            t.Seed(TuningKey.DirReliefCooldown, _cooldownSeconds);
        }

        /// <summary>Refreshes this runtime copy from the live tuning.</summary>
        public void Read(ITuning t)
        {
            _workload = TuningRead.Float(t, TuningKey.DirReliefWorkload, _workload);
            _pressure = TuningRead.Float(t, TuningKey.DirReliefPressure, _pressure);
            _angryCount = Mathf.RoundToInt(TuningRead.Float(t, TuningKey.DirReliefAngryCount, _angryCount));
            _angryWindowSeconds = TuningRead.Float(t, TuningKey.DirReliefAngryWindow, _angryWindowSeconds);
            _holdSeconds = TuningRead.Float(t, TuningKey.DirReliefHold, _holdSeconds);
            _plainCars = Mathf.RoundToInt(TuningRead.Float(t, TuningKey.DirReliefPlainCars, _plainCars));
            _cooldownSeconds = TuningRead.Float(t, TuningKey.DirReliefCooldown, _cooldownSeconds);
        }
    }
}
