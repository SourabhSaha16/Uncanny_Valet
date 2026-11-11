using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// The Lot side of Park Smash. On PowerActivated(ParkSmash) every arrived car that waits for a bay (no driver, not at the
    /// curb, its patron has not asked for it) is stood in a free bay by <see cref="SmashPlan"/>: nose along the bay, centred.
    /// The bay announces CarParked as for any parked car, but no Perfect Park is judged, so the meter is not filled.
    /// Cars that do not fit stay where they are. Publishes ParkSmashCarParked per car, then ParkSmashParked.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SmashRunner : MonoBehaviour
    {
        private const int MaxBlockers = 4;
        private const float BayClearShare = 0.9f;

        private readonly SmashRoster _roster = new SmashRoster();
        private readonly List<SmashCandidate> _all = new List<SmashCandidate>();
        private readonly List<SmashCandidate> _eligible = new List<SmashCandidate>();
        private readonly List<int> _freeRegular = new List<int>();
        private readonly List<int> _freeCharging = new List<int>();
        private readonly List<SmashAssignment> _plan = new List<SmashAssignment>();
        private readonly Dictionary<CarId, ParkingSpot> _placed = new Dictionary<CarId, ParkingSpot>();
        private readonly Collider2D[] _blockers = new Collider2D[MaxBlockers];
        private ContactFilter2D _solid;
        private LotConfig _config;
        private SpotOccupancy _occupancy;
        private ParkingSpot[] _spots;
        private CurbZone _curb;
        private float _aisleY;
        private IEventBus _bus;
        private ICarFactory _cars;

        /// <param name="spots">Every spot; spots[i] has id i + 1.</param>
        /// <param name="aisleY">Local Y of the aisle between the bay rows; decides which way a car faces nose-in.</param>
        public void Init(LotConfig config, SpotOccupancy occupancy, ParkingSpot[] spots, CurbZone curb, float aisleY)
        {
            _config = config;
            _occupancy = occupancy;
            _spots = spots;
            _curb = curb;
            _aisleY = aisleY;
            _solid = new ContactFilter2D { useTriggers = false };
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
            _bus.Unsubscribe<TicketTraitsAssigned>(OnTraits);
            _bus.Unsubscribe<CarArrived>(OnArrived);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<PickupRequested>(OnPickup);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
            _bus.Unsubscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Unsubscribe<ShiftStarted>(OnShiftEdge);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEdge);
            _bus.Unsubscribe<PowerActivated>(OnPower);
        }

        private void Subscribe()
        {
            _bus.Subscribe<TicketTraitsAssigned>(OnTraits);
            _bus.Subscribe<CarArrived>(OnArrived);
            _bus.Subscribe<CarParked>(OnParked);
            _bus.Subscribe<PickupRequested>(OnPickup);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Subscribe<ShiftStarted>(OnShiftEdge);
            _bus.Subscribe<ShiftEnded>(OnShiftEdge);
            _bus.Subscribe<PowerActivated>(OnPower);
        }

        private void OnTraits(TicketTraitsAssigned e) => _roster.Traits(e.Car, e.IsEv, e.IsDirty);

        private void OnArrived(CarArrived e) => _roster.Arrived(e.Car);

        private void OnParked(CarParked e) => _roster.Remove(e.Car);

        private void OnPickup(PickupRequested e) => _roster.Remove(e.Car);

        private void OnLeftSpot(CarLeftSpot e) => _placed.Remove(e.Car);

        private void OnClosed(TicketClosed e)
        {
            _roster.Closed(e.Car);
            if (!_placed.TryGetValue(e.Car, out var spot)) return;
            _placed.Remove(e.Car);
            spot.Release(e.Car); // a placed car may leave before its bay ever settled: free the bay now
        }

        private void OnShiftEdge<T>(T e)
        {
            _roster.Clear();
            _placed.Clear();
        }

        private void OnPower(PowerActivated e)
        {
            if (e.Power != PowerKind.ParkSmash) return;
            _cars = _cars ?? Services.Get<ICarFactory>();
            CollectEligible();
            CollectFreeBays();
            SmashPlan.Assign(_eligible, _freeRegular, _freeCharging, _plan);
            int parked = 0, charged = 0, washed = 0;
            foreach (var a in _plan)
            {
                if (!PlaceCar(_eligible[a.Candidate].Car, a)) continue;
                parked++;
                if (a.Charged) charged++;
                if (a.Washed) washed++;
            }
            _bus.Publish(new ParkSmashParked(parked, charged, washed));
        }

        private void CollectEligible()
        {
            _roster.CopyTo(_all);
            _eligible.Clear();
            foreach (var c in _all)
            {
                if (!_cars.TryGet(c.Car, out var car) || car.Transform == null) continue;
                if (car.HasDriver || _curb.Holds(c.Car)) continue;
                _eligible.Add(c);
            }
        }

        private void CollectFreeBays()
        {
            _freeRegular.Clear();
            _freeCharging.Clear();
            for (var i = 0; i < _spots.Length; i++)
            {
                var id = i + 1;
                if (!_occupancy.IsFree(id) || IsBlocked(_spots[i].transform)) continue;
                (_occupancy.IsCharging(id) ? _freeCharging : _freeRegular).Add(id);
            }
        }

        private bool IsBlocked(Transform bay)
        {
            var size = _config.SpotSize * BayClearShare;
            return Physics2D.OverlapBox(bay.position, size, bay.eulerAngles.z, _solid, _blockers) > 0;
        }

        private bool PlaceCar(CarId id, SmashAssignment a)
        {
            _cars.TryGet(id, out var car);
            var spot = _spots[a.SpotId - 1];
            Teleport(car.Transform, spot.transform, SmashPlan.NoseInDegrees(transform.InverseTransformPoint(spot.transform.position).y, _aisleY));
            _roster.Remove(id);
            if (!spot.Place(id)) return false;
            _placed[id] = spot;
            _bus.Publish(new ParkSmashCarParked(id, a.SpotId, a.Charged, a.Washed));
            return true;
        }

        private static void Teleport(Transform car, Transform bay, float noseInDegrees)
        {
            var angle = bay.eulerAngles.z + noseInDegrees;
            if (car.TryGetComponent<Rigidbody2D>(out var body))
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
                body.position = bay.position;
                body.rotation = angle;
            }
            car.SetPositionAndRotation(bay.position, Quaternion.Euler(0f, 0f, angle));
        }
    }
}
