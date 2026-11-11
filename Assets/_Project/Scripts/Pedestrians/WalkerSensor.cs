using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>Looks a short way ahead of a person's body for solid things (cars, the valet, walls, other people). Reuses its buffer, never allocates.</summary>
    public sealed class WalkerSensor
    {
        private readonly Rigidbody2D _body;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[8];
        private ContactFilter2D _filter;

        public WalkerSensor(Rigidbody2D body)
        {
            _body = body;
            _filter = new ContactFilter2D { useTriggers = false };
        }

        /// <summary>True when a non-trigger collider lies within <paramref name="distance"/> along <paramref name="heading"/>. The person's own car (<paramref name="ownCar"/>) never counts.</summary>
        public bool IsBlocked(Vector2 heading, float distance, CarId ownCar)
        {
            if (heading.sqrMagnitude < Mathf.Epsilon) return false;
            var count = _body.Cast(heading, _filter, _hits, distance);
            for (var i = 0; i < count; i++)
            {
                var car = _hits[i].collider.GetComponentInParent<IDrivable>();
                if (car == null || !OwnCarRule.IsOwn(ownCar, car.Id)) return true;
            }
            return false;
        }
    }
}
