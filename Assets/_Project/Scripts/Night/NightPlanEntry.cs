using System;
using UnityEngine;
using UncannyValet.Core.Events;

namespace UncannyValet.Night
{
    /// <summary>Serializable form of one <see cref="NightPlan"/>, edited in <see cref="NightPlanConfig"/>.</summary>
    [Serializable]
    public sealed class NightPlanEntry
    {
        [SerializeField, Min(1)] private int _night = 1;
        [SerializeField, Min(1)] private int _maxWaitingCars = 1;
        [SerializeField, Min(0.1f), Tooltip("Multiplies the gap between arrivals; below 1 is busier.")]
        private float _arrivalGapScale = 1f;
        [SerializeField, Range(0f, 1f)] private float _evChance;
        [SerializeField, Range(0f, 1f)] private float _dirtyChance;
        [SerializeField] private bool _chargingOpen;
        [SerializeField] private bool _washOpen;
        [SerializeField, TextArea] private string _newTonight = "";

        public NightPlanEntry(NightPlan plan)
        {
            _night = plan.Night; _maxWaitingCars = plan.MaxWaitingCars; _arrivalGapScale = plan.ArrivalGapScale;
            _evChance = plan.EvChance; _dirtyChance = plan.DirtyChance;
            _chargingOpen = plan.ChargingOpen; _washOpen = plan.WashOpen; _newTonight = plan.NewTonight;
        }

        public NightPlan ToPlan() => new NightPlan(_night, _maxWaitingCars, _arrivalGapScale, _evChance,
            _dirtyChance, _chargingOpen, _washOpen, _newTonight);
    }
}
