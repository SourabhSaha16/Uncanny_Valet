using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// A solid box the size of a single-occupancy station that is switched on while a car holds the station, so a
    /// second car cannot drive in. The holder ignores the box (it can drive out); anything that is not a car (the
    /// valet on foot) is let through. A driven car pushing against it publishes StationEntryRefused.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    [DisallowMultipleComponent]
    public sealed class StationBlocker : MonoBehaviour
    {
        private readonly StationOccupancy _occupancy = new StationOccupancy();
        private readonly Dictionary<CarId, IDrivable> _holders = new Dictionary<CarId, IDrivable>();
        private readonly List<Collider2D> _scratch = new List<Collider2D>();
        private BoxCollider2D _box;
        private StationKind _kind;
        private float _repeatSeconds;
        private float _nextRefusalAt;
        private IEventBus _bus;

        /// <param name="repeatSeconds">Least time between two refusal events while a car keeps pushing.</param>
        public void Init(StationKind kind, Vector2 size, float repeatSeconds)
        {
            _kind = kind;
            _repeatSeconds = repeatSeconds;
            _box = GetComponent<BoxCollider2D>();
            _box.size = size;
            _box.enabled = false;
        }

        private void Start() => _bus = Services.Get<IEventBus>();

        /// <summary>A car settled in the station: it now holds it and may leave through the box.</summary>
        public void Admit(IDrivable car)
        {
            if (car == null || !_occupancy.Admit(car.Id)) return;
            _holders[car.Id] = car;
            _box.enabled = true;
            SetIgnored(car, true);
        }

        /// <summary>A holder left (or the station closed): the box switches off once nobody holds the station.</summary>
        public void Release(CarId id)
        {
            if (!_occupancy.Release(id)) return;
            if (_holders.TryGetValue(id, out var car)) SetIgnored(car, false);
            _holders.Remove(id);
            _box.enabled = _occupancy.IsOccupied;
        }

        private void SetIgnored(IDrivable car, bool ignored)
        {
            if (car.Transform == null) return;
            car.Transform.GetComponentsInChildren(_scratch);
            foreach (var c in _scratch) Physics2D.IgnoreCollision(_box, c, ignored);
            _scratch.Clear();
        }

        private void OnCollisionEnter2D(Collision2D hit) => Handle(hit);

        private void OnCollisionStay2D(Collision2D hit) => Handle(hit);

        private void Handle(Collision2D hit)
        {
            var car = hit.collider.GetComponentInParent<IDrivable>();
            if (car == null) { Physics2D.IgnoreCollision(_box, hit.collider); return; }
            if (!car.HasDriver || !_occupancy.BlocksEntryOf(car.Id) || Time.time < _nextRefusalAt) return;
            _nextRefusalAt = Time.time + _repeatSeconds;
            var p = transform.position;
            _bus.Publish(new StationEntryRefused(_kind, p.x, p.y));
        }
    }
}
