using System;
using System.Globalization;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Turns the Park Smash events into the <see cref="SmashFx"/> look: the flash and ring when the power fires (the ring
    /// starts at the valet), a sparkle puff where each car parks, a coin and confetti burst where each patron is paid, and
    /// the SMASH! +$N pop when the smash is complete.
    /// </summary>
    public sealed class SmashFxTracker : IDisposable
    {
        private const string PopPrefix = "SMASH! +$";

        private readonly IEventBus _bus;
        private readonly ICarFactory _cars;
        private readonly Func<float> _clock;
        private IPlayerFocus _focus;

        public SmashFxTracker(IEventBus bus, ICarFactory cars, SmashFx fx, Func<float> clock)
        {
            _bus = bus;
            _cars = cars;
            Fx = fx;
            _clock = clock;
            _bus.Subscribe<PowerActivated>(OnActivated);
            _bus.Subscribe<ParkSmashCarParked>(OnCarParked);
            _bus.Subscribe<ParkSmashCarDelivered>(OnCarDelivered);
            _bus.Subscribe<ParkSmashCompleted>(OnCompleted);
            _bus.Subscribe<ShiftStarted>(OnShiftEdge);
            _bus.Subscribe<ShiftEnded>(OnShiftEdge);
        }

        public SmashFx Fx { get; }

        public void Dispose()
        {
            _bus.Unsubscribe<PowerActivated>(OnActivated);
            _bus.Unsubscribe<ParkSmashCarParked>(OnCarParked);
            _bus.Unsubscribe<ParkSmashCarDelivered>(OnCarDelivered);
            _bus.Unsubscribe<ParkSmashCompleted>(OnCompleted);
            _bus.Unsubscribe<ShiftStarted>(OnShiftEdge);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEdge);
        }

        private void OnActivated(PowerActivated e)
        {
            if (e.Power != PowerKind.ParkSmash) return;
            if (_focus == null) Services.TryGet(out _focus);
            Fx.Begin(_focus != null, _focus != null ? _focus.Position : Vector2.zero, _clock());
        }

        private void OnCarParked(ParkSmashCarParked e)
        {
            if (TryWhere(e.Car, out var at)) Fx.Puff(at, _clock());
        }

        private void OnCarDelivered(ParkSmashCarDelivered e)
        {
            if (TryWhere(e.Car, out var at)) Fx.Burst(at, _clock());
        }

        private void OnCompleted(ParkSmashCompleted e)
        {
            Fx.ShowPop(PopPrefix + e.TotalTips.ToString(CultureInfo.InvariantCulture), _clock());
        }

        private void OnShiftEdge<T>(T e) => Fx.Clear();

        private bool TryWhere(CarId id, out Vector2 at)
        {
            at = Vector2.zero;
            if (!_cars.TryGet(id, out var car) || car.Transform == null) return false;
            at = car.Transform.position;
            return true;
        }
    }
}
