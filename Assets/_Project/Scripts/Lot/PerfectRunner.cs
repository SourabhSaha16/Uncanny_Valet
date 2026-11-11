using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Runs the two perfect judgements and publishes their events. A delivery that Patrons accepted (CarDelivered) is
    /// judged by <see cref="DropOffJudge"/> and publishes PerfectDropOff; a car that comes to rest in a bay (reported by
    /// its <see cref="ParkingSpot"/>) is judged by <see cref="ParkJudge"/> and <see cref="PerfectParkGate"/> and publishes PerfectPark.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PerfectRunner : MonoBehaviour
    {
        private const int MaxContacts = 4;

        private readonly Collider2D[] _contacts = new Collider2D[MaxContacts];
        private readonly CarTicketMap _map = new CarTicketMap();
        private ContactFilter2D _solid;
        private PerfectTuning _tuning;
        private DropOffJudge _dropOff;
        private ParkJudge _park;
        private PerfectParkGate _gate;
        private CurbZone _curb;
        private IEventBus _bus;

        /// <summary>Sets the runner up; <paramref name="curb"/> says how a delivered car rests at the curb.</summary>
        public void Init(LotConfig config, CurbZone curb)
        {
            _tuning = new PerfectTuning(config.Perfect);
            _dropOff = new DropOffJudge(_tuning);
            _park = new ParkJudge(_tuning);
            _gate = new PerfectParkGate(_tuning);
            _curb = curb;
            _solid = new ContactFilter2D { useTriggers = false };
        }

        private void Start()
        {
            _bus = Services.Get<IEventBus>();
            if (Services.TryGet<ITuning>(out var tuning)) _tuning.Apply(tuning);
            Subscribe();
        }

        private void OnEnable()
        {
            if (_bus != null) Subscribe();
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<TicketOpened>(OnOpened);
            _bus.Unsubscribe<TicketTimerChanged>(OnTimer);
            _bus.Unsubscribe<TicketTimerExpired>(OnExpired);
            _bus.Unsubscribe<CarDelivered>(OnDelivered);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
            _bus.Unsubscribe<TuningChanged>(OnTuning);
        }

        private void Subscribe()
        {
            _bus.Subscribe<TicketOpened>(OnOpened);
            _bus.Subscribe<TicketTimerChanged>(OnTimer);
            _bus.Subscribe<TicketTimerExpired>(OnExpired);
            _bus.Subscribe<CarDelivered>(OnDelivered);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<TuningChanged>(OnTuning);
        }

        /// <summary>A car came to rest in this bay (called by the bay once it has published CarParked).</summary>
        public void Parked(Transform bay, IDrivable car)
        {
            var pose = car.Transform;
            var local = bay.InverseTransformPoint(pose.position);
            var angle = Mathf.DeltaAngle(bay.eulerAngles.z, pose.eulerAngles.z);
            var verdict = _park.Judge(angle, local.x, local.y, IsTouching(pose));
            if (!_gate.Rest(car.Id, Time.time, pose.position, verdict == ParkVerdict.Perfect)) return;
            _bus.Publish(new PerfectPark(car.Id, _map.TicketOf(car.Id)));
        }

        private bool IsTouching(Transform car)
        {
            var body = car.GetComponentInChildren<Collider2D>();
            return body != null && body.GetContacts(_solid, _contacts) > 0;
        }

        private void OnDelivered(CarDelivered e)
        {
            if (!_curb.TryGetRotation(e.Car, out var rotation)) return;
            if (_dropOff.IsPerfect(rotation, _curb.Holds(e.Car), _map.PickupElapsed(e.Car)))
                _bus.Publish(new PerfectDropOff(e.Car, e.Ticket));
        }

        private void OnOpened(TicketOpened e) => _map.Opened(e.Car, e.Ticket);

        private void OnTimer(TicketTimerChanged e)
        {
            if (e.Kind == TimerKind.Pickup) _map.PickupProgress(e.Car, e.Elapsed01);
        }

        private void OnExpired(TicketTimerExpired e)
        {
            if (e.Kind == TimerKind.Pickup) _map.PickupProgress(e.Car, 1f);
        }

        private void OnClosed(TicketClosed e)
        {
            _map.Closed(e.Car);
            _gate.Forget(e.Car);
        }

        private void OnTuning(TuningChanged e) => _tuning.TrySet(e.Key, e.Value);
    }
}
