using UncannyValet.Core;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Looks ahead of a trip car with its own collider: the bumper gap to the nearest solid body (a car, the valet) it is
    /// driving toward. Triggers, pedestrians (their own rule), static scenery and things beside or behind it do not count.
    /// Reuses its buffer, so it never allocates.
    /// </summary>
    public sealed class TripProbe
    {
        private const int MaxHits = 8;

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[MaxHits];
        private ContactFilter2D _filter;

        public TripProbe()
        {
            _filter = new ContactFilter2D();
            _filter.useTriggers = false;
            _filter.SetLayerMask(~PhysicsLayers.PedestrianMask);
        }

        /// <summary>The gap along <paramref name="heading"/> within <paramref name="distance"/>, or infinity when the way is clear.</summary>
        public float Gap(Rigidbody2D body, Vector2 heading, float distance)
        {
            int count = body.Cast(heading, _filter, _hits, distance);
            float best = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = _hits[i];
                if (hit.rigidbody == null || Vector2.Dot(hit.normal, heading) >= 0f) continue;
                best = Mathf.Min(best, hit.distance);
            }
            return best;
        }
    }
}
