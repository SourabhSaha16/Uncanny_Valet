using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>
    /// The drop-off zone. Publishes CarAtCurb once an empty car settles inside it facing up the screen (within the
    /// configured tolerance). A resting car that faces the wrong way is not accepted; CurbFacingChanged tells the UI to
    /// show a hint until the car is turned or leaves.
    /// </summary>
    public class CurbZone : StopZone
    {
        private readonly HashSet<CarId> _wrong = new HashSet<CarId>();
        private ICurbFacing _facing;

        private readonly Dictionary<CarId, bool> _facingByCar = new Dictionary<CarId, bool>();
        private CurbArrow _arrow;

        /// <summary>Sets the zone up; <paramref name="arrow"/> (optional) is tinted by how the cars inside face.</summary>
        public void Init(LotConfig config, CurbArrow arrow = null)
        {
            Setup(config, config.CurbSize);
            _facing = new CurbFacingRule(config.CurbFacingToleranceDegrees);
            _arrow = arrow;
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
            TrackFacing(car.Id, facing);
            return facing;
        }

        protected override void OnSettled(CarId car) => Bus.Publish(new CarAtCurb(car));

        protected override void OnLeft(CarId car) { }

        protected override void OnForget(CarId car)
        {
            SetWrong(car, false);
            if (_facingByCar.Remove(car)) ShowArrow();
        }

        private void TrackFacing(CarId car, bool facing)
        {
            if (_facingByCar.TryGetValue(car, out var known) && known == facing) return;
            _facingByCar[car] = facing;
            ShowArrow();
        }

        private void ShowArrow()
        {
            if (_arrow == null) return;
            var right = 0;
            foreach (var facing in _facingByCar.Values) if (facing) right++;
            _arrow.Show(CurbArrowRule.Evaluate(right, _facingByCar.Count - right));
        }

        private void SetWrong(CarId car, bool wrong)
        {
            var changed = wrong ? _wrong.Add(car) : _wrong.Remove(car);
            if (changed) Bus.Publish(new CurbFacingChanged(car, wrong));
        }
    }
}
