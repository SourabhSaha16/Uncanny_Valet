using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Asks the physics world whether a walking leg would run into a car standing where it is now (parked, queued, charging). The
    /// body-wide sweep means a person never plans a walk through a bay. Reuses its buffer, allocates nothing.
    /// </summary>
    public sealed class CarLegProbe : ILegBlocker
    {
        private readonly float _radius;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[8];
        private ContactFilter2D _filter = new ContactFilter2D { useTriggers = false };

        /// <param name="bodyRadius">Radius of a person's body (world units); the sweep is this wide.</param>
        public CarLegProbe(float bodyRadius) => _radius = Mathf.Max(0f, bodyRadius);

        /// <summary>True when a body-wide sweep along any piece of the walk touches a car.</summary>
        public bool Crosses(IReadOnlyList<Vector2> walk)
        {
            for (var i = 1; i < walk.Count; i++)
            {
                var d = walk[i] - walk[i - 1];
                var length = d.magnitude;
                if (length < Mathf.Epsilon) continue;
                var count = Physics2D.CircleCast(walk[i - 1], _radius, d / length, _filter, _hits, length);
                for (var h = 0; h < count; h++)
                    if (_hits[h].collider.GetComponentInParent<IDrivable>() != null) return true;
            }
            return false;
        }
    }
}
