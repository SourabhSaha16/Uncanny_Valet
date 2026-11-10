using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>The weight multipliers and thresholds of the seven dilemma rules (spec section 4).</summary>
    [Serializable]
    public sealed class DilemmaRuleSettings
    {
        [SerializeField, Min(1f), Tooltip("Wash slot holds a washed car: Dirty weight multiplier.")]
        private float _washBlocked = 3f;
        [SerializeField, Min(1f), Tooltip("Wash slot mid-wash with nothing queued: Dirty weight multiplier.")]
        private float _washBusy = 1.5f;
        [SerializeField, Min(1f), Tooltip("Every EV bay holds a fully charged EV: EV weight multiplier.")]
        private float _baysBlocked = 3f;
        [SerializeField, Min(1f), Tooltip("A plain car squats in an EV bay: EV weight multiplier.")]
        private float _baySquatted = 2.5f;
        [SerializeField, Min(1f), Tooltip("The last EV bay is free while the others (or, with a single bay, the wash) are busy: EV weight multiplier.")]
        private float _lastBay = 1.5f;
        [SerializeField, Min(1f), Tooltip("Lot tight: Plain weight multiplier.")]
        private float _lotTight = 1.5f;
        [SerializeField, Min(1f), Tooltip("The wash slot and every EV bay hold finished cars: Dirty EV weight multiplier.")]
        private float _chain = 2f;
        [SerializeField, Min(0), Tooltip("The lot counts as tight with this many free regular spots or fewer.")]
        private int _lotTightSpots = 1;
        [SerializeField, Min(0f), Tooltip("A dilemma with no free spot is still solvable when a pickup is due within this many seconds.")]
        private float _solvablePickupSeconds = 10f;

        public float WashBlocked => _washBlocked;
        public float WashBusy => _washBusy;
        public float BaysBlocked => _baysBlocked;
        public float BaySquatted => _baySquatted;
        public float LastBay => _lastBay;
        public float LotTight => _lotTight;
        public float Chain => _chain;
        public int LotTightSpots => _lotTightSpots;
        public float SolvablePickupSeconds => _solvablePickupSeconds;

        /// <summary>Writes the starting values into the live tuning (Bootstrap only).</summary>
        public void Seed(ITuning t)
        {
            t.Seed(TuningKey.DirDilemmaWashBlocked, _washBlocked);
            t.Seed(TuningKey.DirDilemmaWashBusy, _washBusy);
            t.Seed(TuningKey.DirDilemmaBaysBlocked, _baysBlocked);
            t.Seed(TuningKey.DirDilemmaBaySquatted, _baySquatted);
            t.Seed(TuningKey.DirDilemmaLastBay, _lastBay);
            t.Seed(TuningKey.DirDilemmaLotTight, _lotTight);
            t.Seed(TuningKey.DirDilemmaChain, _chain);
            t.Seed(TuningKey.DirLotTightSpots, _lotTightSpots);
            t.Seed(TuningKey.DirSolvablePickupSeconds, _solvablePickupSeconds);
        }

        /// <summary>Refreshes this runtime copy from the live tuning.</summary>
        public void Read(ITuning t)
        {
            _washBlocked = TuningRead.Float(t, TuningKey.DirDilemmaWashBlocked, _washBlocked);
            _washBusy = TuningRead.Float(t, TuningKey.DirDilemmaWashBusy, _washBusy);
            _baysBlocked = TuningRead.Float(t, TuningKey.DirDilemmaBaysBlocked, _baysBlocked);
            _baySquatted = TuningRead.Float(t, TuningKey.DirDilemmaBaySquatted, _baySquatted);
            _lastBay = TuningRead.Float(t, TuningKey.DirDilemmaLastBay, _lastBay);
            _lotTight = TuningRead.Float(t, TuningKey.DirDilemmaLotTight, _lotTight);
            _chain = TuningRead.Float(t, TuningKey.DirDilemmaChain, _chain);
            _lotTightSpots = Mathf.RoundToInt(TuningRead.Float(t, TuningKey.DirLotTightSpots, _lotTightSpots));
            _solvablePickupSeconds = TuningRead.Float(t, TuningKey.DirSolvablePickupSeconds, _solvablePickupSeconds);
        }
    }
}
