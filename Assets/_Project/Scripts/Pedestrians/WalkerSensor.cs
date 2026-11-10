using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>Looks a short way ahead of a person's body for solid things (cars, the valet, walls, other people). Reuses its buffer, never allocates.</summary>
    public sealed class WalkerSensor
    {
        private readonly Rigidbody2D _body;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[4];
        private ContactFilter2D _filter;

        public WalkerSensor(Rigidbody2D body)
        {
            _body = body;
            _filter = new ContactFilter2D { useTriggers = false };
        }

        /// <summary>True when a non-trigger collider lies within <paramref name="distance"/> along <paramref name="heading"/>.</summary>
        public bool IsBlocked(Vector2 heading, float distance)
        {
            if (heading.sqrMagnitude < Mathf.Epsilon) return false;
            return _body.Cast(heading, _filter, _hits, distance) > 0;
        }
    }
}
