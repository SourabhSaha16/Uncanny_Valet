using UncannyValet.Core;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Shows and hides the wash slot to match the night plan. Closing it first releases the cars in it
    /// (so CarLeftStation is published) and then deactivates it. It starts closed.
    /// </summary>
    public sealed class StationSwitch : MonoBehaviour
    {
        private StationZone _wash;
        private IEventBus _bus;

        public void Init(StationZone wash) => _wash = wash;

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
            var open = e.Plan.WashOpen;
            if (_wash.gameObject.activeSelf == open) return;
            if (!open) _wash.ReleaseAll();
            _wash.gameObject.SetActive(open);
        }
    }
}
