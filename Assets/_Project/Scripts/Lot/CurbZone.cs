using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>
    /// The drop-off zone. Publishes CarAtCurb once an empty car settles inside it facing up the screen (within the
    /// configured tolerance). A resting car that faces the wrong way is not accepted; CurbFacingChanged tells the UI to
    /// show a hint until the car is turned or leaves. The pickup arrows turn green only when a correctly facing car's
    /// patron will take it (IPickupAcceptance), and re-evaluate when a PickupRequested arrives or a ticket closes (the patron no longer waits).
    /// </summary>
    public class CurbZone : StopZone
    {
        private readonly HashSet<CarId> _wrong = new HashSet<CarId>();
        private ICurbFacing _facing;
        private readonly CurbArrowTracker _tracker = new CurbArrowTracker();
        private CurbArrow _arrow;
        private IEventBus _bus;
        private IPickupAcceptance _accept;

        private void Start()
        {
            _bus = Bus;
            _bus.Subscribe<PickupRequested>(OnPickupRequested);
            _bus.Subscribe<TicketClosed>(OnTicketClosed);
        }

        private void OnEnable()
        {
            if (_bus == null) return;
            _bus.Subscribe<PickupRequested>(OnPickupRequested);
            _bus.Subscribe<TicketClosed>(OnTicketClosed);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<PickupRequested>(OnPickupRequested);
            _bus.Unsubscribe<TicketClosed>(OnTicketClosed);
        }

        private void OnPickupRequested(PickupRequested e) => ShowArrow();

        private void OnTicketClosed(TicketClosed e) => ShowArrow();

        /// <summary>Sets the zone up; <paramref name="arrow"/> (optional) is tinted by how the cars inside face.</summary>
        public void Init(LotConfig config, CurbArrow arrow = null) => Init(config, config.CurbSize, arrow);

        /// <summary>Same, for a curb zone of its own <paramref name="size"/> instead of the config's curb size.</summary>
        public void Init(LotConfig config, UnityEngine.Vector2 size, CurbArrow arrow = null)
        {
            Setup(config, size);
            _facing = new CurbFacingRule(config.CurbFacingToleranceDegrees);
            _arrow = arrow;
            Span = arrow == null ? CurbArrowSpan.None : LotCurbArrows.SpanOf(config, size);
        }

        /// <summary>The strip between the painted arrows; none when the zone has no arrows (then perfect never waits on them).</summary>
        public CurbArrowSpan Span { get; private set; }

        /// <summary>Sideways distance of a tracked car's centre from the zone centre; false when the zone no longer tracks it.</summary>
        public bool TryGetOffsetX(CarId car, out float offsetX)
        {
            offsetX = 0f;
            if (!TryGetCar(car, out var drivable)) return false;
            offsetX = transform.InverseTransformPoint(drivable.Transform.position).x;
            return true;
        }

        /// <summary>Rotation in degrees (0 = nose up) of a car the zone still tracks; false when it is not here.</summary>
        public bool TryGetRotation(CarId car, out float degrees)
        {
            degrees = 0f;
            if (!TryGetCar(car, out var drivable)) return false;
            degrees = drivable.Transform.eulerAngles.z;
            return true;
        }

        protected override bool Accepts(IDrivable car, bool resting)
        {
            var facing = _facing.IsAcceptable(car.Transform.eulerAngles.z);
            SetWrong(car.Id, resting && !facing);
            if (_tracker.Update(car.Id, CentreInside(car), facing)) ShowArrow();
            return facing;
        }

        protected override void OnSettled(CarId car) => Bus.Publish(new CarAtCurb(car));

        protected override void OnLeft(CarId car) { }

        protected override void OnForget(CarId car)
        {
            SetWrong(car, false);
            if (_tracker.Remove(car)) ShowArrow();
        }

        private void ShowArrow()
        {
            if (_arrow == null) return;
            if (_accept == null) Services.TryGet(out _accept);
            _arrow.Show(_tracker.Evaluate(_accept));
        }

        private void SetWrong(CarId car, bool wrong)
        {
            var changed = wrong ? _wrong.Add(car) : _wrong.Remove(car);
            if (changed) Bus.Publish(new CurbFacingChanged(car, wrong));
        }
    }
}
