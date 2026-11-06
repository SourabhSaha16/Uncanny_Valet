using UncannyValet.Core;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Closes the spots beyond the night plan's ActiveSpots: tells <see cref="SpotOccupancy"/> and shows a
    /// dark cover over each closed spot. Spots start open until the first plan arrives.
    /// </summary>
    public sealed class SpotCloser : MonoBehaviour
    {
        private SpotOccupancy _occupancy;
        private GameObject[] _covers = new GameObject[0];
        private IEventBus _bus;

        /// <param name="covers">Cover object per spot; covers[i] belongs to spot id i + 1. Must start inactive.</param>
        public void Init(SpotOccupancy occupancy, GameObject[] covers)
        {
            _occupancy = occupancy;
            _covers = covers;
        }

        private void Start()
        {
            _bus = Services.Get<IEventBus>();
            _bus.Subscribe<NightPlanChanged>(OnPlan);
        }

        private void OnEnable()
        {
            if (_bus != null) _bus.Subscribe<NightPlanChanged>(OnPlan);
        }

        private void OnDisable()
        {
            if (_bus != null) _bus.Unsubscribe<NightPlanChanged>(OnPlan);
        }

        private void OnPlan(NightPlanChanged e)
        {
            _occupancy.SetActiveSpots(e.Plan.ActiveSpots);
            for (var i = 0; i < _covers.Length; i++)
                _covers[i].SetActive(!_occupancy.IsOpen(i + 1));
        }
    }
}
