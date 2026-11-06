using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Makes the lot match the night plan: only open spots exist (the others are switched off), and the
    /// ground and walls move to fit what is open. Reacts to every NightPlanChanged and to cars leaving a
    /// spot (a spot kept open by its car closes once it is empty). Never allocates after setup.
    /// </summary>
    public sealed class LotExpander : MonoBehaviour
    {
        private readonly List<int> _openIds = new List<int>();
        private SpotOccupancy _occupancy;
        private LotGeometry _geometry;
        private ParkingSpot[] _spots = new ParkingSpot[0];
        private LotFrame _frame;
        private IEventBus _bus;
        private bool _washOpen;

        /// <summary>Ids of the spots that are open now (regular and charging), ascending.</summary>
        public IReadOnlyList<int> OpenIds => _openIds;

        /// <summary>The current frame inside the walls, in lot-local coordinates.</summary>
        public Rect Frame { get; private set; }

        /// <param name="spots">Every spot; spots[i] has id i + 1 (regular first, then charging).</param>
        public void Init(SpotOccupancy occupancy, LotGeometry geometry, ParkingSpot[] spots, LotFrame frame)
        {
            _occupancy = occupancy;
            _geometry = geometry;
            _spots = spots;
            _frame = frame;
            Refresh();
        }

        private void Start()
        {
            _bus = Services.Get<IEventBus>();
            Subscribe();
        }

        private void OnEnable()
        {
            if (_bus != null) Subscribe();
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<NightPlanChanged>(OnPlan);
            _bus.Unsubscribe<CarLeftSpot>(OnLeftSpot);
        }

        private void Subscribe()
        {
            _bus.Subscribe<NightPlanChanged>(OnPlan);
            _bus.Subscribe<CarLeftSpot>(OnLeftSpot);
        }

        private void OnPlan(NightPlanChanged e)
        {
            _occupancy.SetActiveSpots(e.Plan.ActiveSpots);
            _occupancy.SetChargingOpen(e.Plan.ChargingOpen);
            _washOpen = e.Plan.WashOpen;
            Refresh();
        }

        private void OnLeftSpot(CarLeftSpot e) => Refresh();

        private void Refresh()
        {
            _openIds.Clear();
            for (var i = 0; i < _spots.Length; i++)
            {
                var id = i + 1;
                var open = _occupancy.IsOpen(id);
                if (open) _openIds.Add(id);
                if (_spots[i].gameObject.activeSelf == open) continue;
                if (!open) _spots[i].ReleaseAll();
                _spots[i].gameObject.SetActive(open);
            }
            Frame = LotFrameRule.Frame(_geometry, _occupancy, _washOpen);
            _frame.Apply(Frame);
        }
    }
}
