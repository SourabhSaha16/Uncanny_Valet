using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Reacts to PowerActivated(ThunderStride) while the valet drives a car: puts a <see cref="ThunderBoostModifier"/> built from
    /// the level's numbers on that car (top speed in PushStrength, acceleration in Width) and takes it off when the power ends,
    /// the valet gets out, or the car is gone. Collisions and damage are untouched.
    /// </summary>
    public sealed class ThunderBoostLink : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ICarFactory _cars;
        private readonly Func<IPowerView> _view;
        private CarId _drivenCar;
        private bool _driving;
        private CarDriver _boosted;
        private ThunderBoostModifier _modifier;

        /// <param name="view">Resolves the power numbers; may return null before the Powers module has registered them.</param>
        public ThunderBoostLink(IEventBus bus, ICarFactory cars, Func<IPowerView> view)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _cars = cars ?? throw new ArgumentNullException(nameof(cars));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _bus.Subscribe<CarEntered>(OnEntered);
            _bus.Subscribe<CarExited>(OnExited);
            _bus.Subscribe<PowerActivated>(OnActivated);
            _bus.Subscribe<PowerEnded>(OnEnded);
        }

        /// <summary>True while a boost modifier is on a car.</summary>
        public bool IsBoosting => _modifier != null;

        /// <summary>Stops listening and removes any boost.</summary>
        public void Dispose()
        {
            _bus.Unsubscribe<CarEntered>(OnEntered);
            _bus.Unsubscribe<CarExited>(OnExited);
            _bus.Unsubscribe<PowerActivated>(OnActivated);
            _bus.Unsubscribe<PowerEnded>(OnEnded);
            Clear();
        }

        private void OnEntered(CarEntered e) { _drivenCar = e.Car; _driving = true; }

        private void OnExited(CarExited e)
        {
            if (_driving && e.Car == _drivenCar) { _driving = false; Clear(); }
        }

        private void OnEnded(PowerEnded e)
        {
            if (e.Power == StoryPower.ThunderStride) Clear();
        }

        private void OnActivated(PowerActivated e)
        {
            if (e.Power != StoryPower.ThunderStride || !_driving) return;
            var view = _view();
            if (view == null || !view.TryGetValues(e.Power, e.Level, out var values)) return;
            if (!_cars.TryGet(_drivenCar, out var car) || car.Transform == null || !car.Transform.TryGetComponent(out CarDriver driver)) return;
            Clear();
            _boosted = driver;
            _modifier = new ThunderBoostModifier(values.PushStrength, values.Width);
            driver.Modifiers.Add(_modifier);
        }

        private void Clear()
        {
            if (_modifier == null) return;
            if (_boosted != null) _boosted.Modifiers.Remove(_modifier);
            _boosted = null;
            _modifier = null;
        }
    }
}
