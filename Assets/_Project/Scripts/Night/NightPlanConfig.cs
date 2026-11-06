using System.Collections.Generic;
using UnityEngine;
using UncannyValet.Core.Events;

namespace UncannyValet.Night
{
    /// <summary>One <see cref="NightPlan"/> per night. An empty list is filled from <see cref="NightPlanDefaults"/>.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Night Plan Config", fileName = "NightPlanConfig")]
    public sealed class NightPlanConfig : ScriptableObject
    {
        [SerializeField] private List<NightPlanEntry> _nights = new List<NightPlanEntry>();

        private void Reset() => FillDefaults();

        private void OnValidate()
        {
            if (_nights.Count == 0) FillDefaults();
        }

        private void FillDefaults()
        {
            _nights.Clear();
            foreach (var plan in NightPlanDefaults.Create()) _nights.Add(new NightPlanEntry(plan));
        }

        /// <summary>The plan for a 1-based night; nights past the last entry reuse the last one.</summary>
        public NightPlan For(int night)
        {
            if (_nights.Count == 0) return NightPlanDefaults.For(night);
            return _nights[Mathf.Clamp(night - 1, 0, _nights.Count - 1)].ToPlan();
        }
    }
}
