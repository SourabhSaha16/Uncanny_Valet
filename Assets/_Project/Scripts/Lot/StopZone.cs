using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Trigger zone that notices cars which are stopped, empty and centred inside it for the
    /// configured time. Subclasses decide which event to publish.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class StopZone : MonoBehaviour
    {
        private readonly Dictionary<IDrivable, StopTimer> _inside = new Dictionary<IDrivable, StopTimer>();
        private readonly List<IDrivable> _scratch = new List<IDrivable>();
        private BoxCollider2D _box;
        private LotConfig _config;

        protected IEventBus Bus => Services.Get<IEventBus>();

        protected void Setup(LotConfig config, Vector2 size)
        {
            _config = config;
            _box = GetComponent<BoxCollider2D>();
            _box.isTrigger = true;
            _box.size = size;
        }

        protected abstract void OnSettled(IDrivable car);
        protected abstract void OnLeft(IDrivable car);

        private void OnTriggerEnter2D(Collider2D other)
        {
            var car = other.GetComponentInParent<IDrivable>();
            if (car != null && !_inside.ContainsKey(car)) _inside[car] = new StopTimer(_config.SettleSeconds);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var car = other.GetComponentInParent<IDrivable>();
            if (car == null || !_inside.TryGetValue(car, out var timer)) return;
            _inside.Remove(car);
            if (timer.IsDone) OnLeft(car);
        }

        private void Update()
        {
            _scratch.AddRange(_inside.Keys);
            foreach (var car in _scratch)
            {
                if (car is Object unityObject && unityObject == null)
                {
                    _inside.Remove(car); // destroyed while inside; no exit callback will arrive
                    continue;
                }
                var stopped = !car.HasDriver && car.Speed < _config.StoppedSpeed
                              && _box.OverlapPoint(car.Transform.position);
                if (_inside[car].Tick(Time.deltaTime, stopped)) OnSettled(car);
            }
            _scratch.Clear();
        }
    }
}
