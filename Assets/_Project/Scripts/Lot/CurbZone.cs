using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>
    /// The drop-off zone. Publishes CarAtCurb once an empty car settles inside it facing down the screen (within the
    /// configured tolerance). A resting car that faces the wrong way is not accepted; CurbFacingChanged tells the UI to
    /// show a hint until the car is turned or leaves.
    /// </summary>
    public class CurbZone : StopZone
    {
        private readonly HashSet<CarId> _wrong = new HashSet<CarId>();
        private ICurbFacing _facing;

        public void Init(LotConfig config)
        {
            Setup(config, config.CurbSize);
            _facing = new CurbFacingRule(config.CurbFacingToleranceDegrees);
        }

        protected override bool Accepts(IDrivable car, bool resting)
        {
            var facing = _facing.IsAcceptable(car.Transform.eulerAngles.z);
            SetWrong(car.Id, resting && !facing);
            return facing;
        }

        protected override void OnSettled(CarId car) => Bus.Publish(new CarAtCurb(car));

        protected override void OnLeft(CarId car) { }

        protected override void OnForget(CarId car) => SetWrong(car, false);

        private void SetWrong(CarId car, bool wrong)
        {
            var changed = wrong ? _wrong.Add(car) : _wrong.Remove(car);
            if (changed) Bus.Publish(new CurbFacingChanged(car, wrong));
        }
    }
}
