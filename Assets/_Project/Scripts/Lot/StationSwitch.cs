using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Shows and hides the wash stations (the wash slot, or every trough) to match the night plan. Closing them first releases the cars in
    /// them (so CarLeftStation is published) and then deactivates them. They start closed.
    /// </summary>
    public sealed class StationSwitch : MonoBehaviour
    {
        private readonly List<StationZone> _washes = new List<StationZone>();
        private IEventBus _bus;

        public void Init(StationZone wash) => _washes.Add(wash);

        private void Start()
        {
            _bus = Services.Get<IEventBus>();
            _bus.Subscribe<ShiftPlanChanged>(OnPlan);
        }

        private void OnEnable()
        {
            if (_bus != null) _bus.Subscribe<ShiftPlanChanged>(OnPlan);
        }

        private void OnDisable()
        {
            if (_bus != null) _bus.Unsubscribe<ShiftPlanChanged>(OnPlan);
        }

        private void OnPlan(ShiftPlanChanged e)
        {
            var open = e.Plan.WashOpen;
            for (var i = 0; i < _washes.Count; i++)
            {
                if (_washes[i].gameObject.activeSelf == open) continue;
                if (!open) _washes[i].ReleaseAll();
                _washes[i].gameObject.SetActive(open);
            }
        }
    }
}
