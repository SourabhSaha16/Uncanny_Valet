using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure rule for the glow of one wash or charging slot: dark while idle, lit in proportion to the service progress
    /// while the car inside is being washed or charged, then a short fading flash once the service finished. Levels
    /// reported before the car reached the slot are remembered, so the glow starts the moment the car settles.
    /// </summary>
    public sealed class SlotTintRule
    {
        private readonly Dictionary<CarId, float> _levels = new Dictionary<CarId, float>();
        private readonly float _minShare;
        private readonly float _maxShare;
        private readonly float _doneSeconds;
        private bool _hasCar;
        private CarId _car;
        private float _doneElapsed;

        public SlotTintRule(float minShare, float maxShare, float doneSeconds)
        {
            _minShare = minShare;
            _maxShare = maxShare;
            _doneSeconds = doneSeconds;
        }

        /// <summary>Whether the slot is lit, working or flashing done.</summary>
        public SlotTintPhase Phase { get; private set; }

        /// <summary>How far from the idle colour to the lit colour: 0 idle, up to the configured maximum.</summary>
        public float Share { get; private set; }

        /// <summary>The progress of a car's service (0 not started, 1 complete), wherever the car is.</summary>
        public void Progress(CarId car, float level01)
        {
            _levels[car] = level01;
            if (_hasCar && _car.Equals(car) && Phase != SlotTintPhase.Done) Work(level01);
        }

        /// <summary>A car settled in the slot; it lights up when the car has a service in progress.</summary>
        public void Enter(CarId car)
        {
            _car = car;
            _hasCar = true;
            Phase = SlotTintPhase.Idle;
            Share = 0f;
            if (_levels.TryGetValue(car, out var level)) Work(level);
        }

        /// <summary>The service finished (the car is washed or fully charged).</summary>
        public void Finished(CarId car)
        {
            _levels.Remove(car);
            if (!_hasCar || !_car.Equals(car)) return;
            Phase = SlotTintPhase.Done;
            _doneElapsed = 0f;
            Share = _maxShare;
        }

        /// <summary>The car left the slot (driven out, destroyed or the slot closed): the glow goes out at once.</summary>
        public void Leave(CarId car)
        {
            if (!_hasCar || !_car.Equals(car)) return;
            _hasCar = false;
            Phase = SlotTintPhase.Idle;
            Share = 0f;
        }

        /// <summary>The ticket was closed: forget the car and clear the glow.</summary>
        public void Closed(CarId car)
        {
            _levels.Remove(car);
            Leave(car);
        }

        /// <summary>Advances the done flash by <paramref name="dt"/> seconds.</summary>
        public void Tick(float dt)
        {
            if (Phase != SlotTintPhase.Done) return;
            _doneElapsed += dt;
            var left = 1f - _doneElapsed / _doneSeconds;
            if (left <= 0f) { Phase = SlotTintPhase.Idle; Share = 0f; }
            else Share = _maxShare * left;
        }

        private void Work(float level01)
        {
            if (level01 >= 1f && Phase == SlotTintPhase.Idle) { Share = 0f; return; }
            var clamped = level01 < 0f ? 0f : level01 > 1f ? 1f : level01;
            Phase = SlotTintPhase.Working;
            Share = _minShare + (_maxShare - _minShare) * clamped;
        }
    }
}
