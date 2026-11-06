using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Trigger zone that notices cars which are stopped, empty and centred inside it for the
    /// configured time. Subclasses decide which event to publish. A car destroyed while inside is
    /// released as if it had left.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class StopZone : MonoBehaviour
    {
        private readonly Dictionary<CarId, IDrivable> _cars = new Dictionary<CarId, IDrivable>();
        private readonly List<CarId> _settled = new List<CarId>();
        private readonly List<(CarId id, bool wasSettled)> _gone = new List<(CarId id, bool wasSettled)>();
        private ZoneRoster _roster;
        private BoxCollider2D _box;
        private LotConfig _config;

        protected IEventBus Bus => Services.Get<IEventBus>();

        protected void Setup(LotConfig config, Vector2 size)
        {
            _config = config;
            _roster = new ZoneRoster(config.SettleSeconds);
            _box = GetComponent<BoxCollider2D>();
            _box.isTrigger = true;
            _box.size = size;
        }

        protected abstract void OnSettled(CarId car);
        protected abstract void OnLeft(CarId car);

        private void OnTriggerEnter2D(Collider2D other)
        {
            var car = other.GetComponentInParent<IDrivable>();
            if (car == null) return;
            _cars[car.Id] = car;
            _roster.Enter(car.Id);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var car = other.GetComponentInParent<IDrivable>();
            if (car == null) return;
            if (_roster.Exit(car.Id)) OnLeft(car.Id);
            _cars.Remove(car.Id);
        }

        private CarReading Read(CarId id)
        {
            var car = _cars[id];
            if (car is Object unityObject && unityObject == null) return CarReading.Gone;
            var stopped = !car.HasDriver && car.Speed < _config.StoppedSpeed
                          && _box.OverlapPoint(car.Transform.position);
            return stopped ? CarReading.Stopped : CarReading.Active;
        }

        private void Update()
        {
            if (_roster == null || _roster.Count == 0) return;
            _roster.Tick(Time.deltaTime, Read, _settled, _gone);
            foreach (var id in _settled) OnSettled(id);
            foreach (var (id, wasSettled) in _gone)
            {
                _cars.Remove(id);
                if (wasSettled) OnLeft(id);
            }
            _settled.Clear();
            _gone.Clear();
        }
    }
}
