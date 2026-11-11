using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>Turns PerfectPark and PerfectDropOff into a camera kick along the car's nose direction.</summary>
    public sealed class PerfectKickLink : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ICarFactory _cars;
        private readonly Action<Vector2> _kick;

        /// <param name="kick">Receives the unit direction the car faces.</param>
        public PerfectKickLink(IEventBus bus, ICarFactory cars, Action<Vector2> kick)
        {
            _bus = bus;
            _cars = cars;
            _kick = kick;
            _bus.Subscribe<PerfectPark>(OnPerfectPark);
            _bus.Subscribe<PerfectDropOff>(OnPerfectDropOff);
        }

        /// <summary>Stops listening.</summary>
        public void Dispose()
        {
            _bus.Unsubscribe<PerfectPark>(OnPerfectPark);
            _bus.Unsubscribe<PerfectDropOff>(OnPerfectDropOff);
        }

        private void OnPerfectPark(PerfectPark e) => Kick(e.Car);

        private void OnPerfectDropOff(PerfectDropOff e) => Kick(e.Car);

        private void Kick(CarId id)
        {
            if (_cars.TryGet(id, out var car) && car.Transform != null) _kick(car.Transform.up);
        }
    }
}
