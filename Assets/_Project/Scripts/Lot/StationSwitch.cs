using UncannyValet.Core;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Shows and hides the stations to match the night plan. Closing a station first releases the cars in
    /// it (so CarLeftStation is published) and then deactivates it. Stations start closed.
    /// </summary>
    public sealed class StationSwitch : MonoBehaviour
    {
        private StationZone[] _zones = new StationZone[0];
        private IEventBus _bus;

        public void Init(StationZone[] zones) => _zones = zones;

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
            foreach (var zone in _zones)
                Apply(zone, zone.Kind == StationKind.Charging ? e.Plan.ChargingOpen : e.Plan.WashOpen);
        }

        private static void Apply(StationZone zone, bool open)
        {
            if (zone.gameObject.activeSelf == open) return;
            if (!open) zone.ReleaseAll();
            zone.gameObject.SetActive(open);
        }
    }
}
