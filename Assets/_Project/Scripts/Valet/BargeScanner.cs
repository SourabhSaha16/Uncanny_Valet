using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>Looks for pedestrians overlapping the boosted valet and publishes ValetBargedPedestrian for each one the rule lets through. Reuses its buffer.</summary>
    public sealed class BargeScanner
    {
        private readonly Collider2D[] _hits = new Collider2D[BufferSize];
        private readonly ContactFilter2D _filter;
        private readonly BargeRule _rule;
        private readonly IEventBus _bus;

        private const int BufferSize = 8;

        public BargeScanner(IEventBus bus, BargeRule rule)
        {
            _bus = bus;
            _rule = rule;
            _filter = new ContactFilter2D { useLayerMask = true, layerMask = PhysicsLayers.PedestrianMask, useTriggers = false };
        }

        /// <summary>Barges every person within <paramref name="radius"/> of <paramref name="centre"/> that the rule allows.</summary>
        public void Scan(Vector2 centre, float radius, float now)
        {
            var count = Physics2D.OverlapCircle(centre, radius, _filter, _hits);
            for (var i = 0; i < count; i++)
            {
                var id = _hits[i].gameObject.GetInstanceID();
                if (!_rule.TryBarge(true, id, now)) continue;
                var push = (Vector2)_hits[i].transform.position - centre;
                push = push.sqrMagnitude > Mathf.Epsilon ? push.normalized : Vector2.up;
                _bus.Publish(new ValetBargedPedestrian(new PedestrianId(id), push.x, push.y));
            }
        }
    }
}
