using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure rule for "parked completely inside a bay": every given point (the car's collider corners, in the bay's local
    /// space) lies within the bay rectangle grown by a small tolerance. Touching the line counts as inside; sticking out
    /// by more than the tolerance does not.
    /// </summary>
    public sealed class BayFitRule
    {
        private readonly Vector2 _half;
        private readonly float _tolerance;

        /// <param name="bayHalfSize">Half width and half length of the bay.</param>
        /// <param name="tolerance">How far a point may stray past the bay edge and still count as inside.</param>
        public BayFitRule(Vector2 bayHalfSize, float tolerance)
        {
            _half = bayHalfSize;
            _tolerance = tolerance;
        }

        /// <summary>True when every point is inside the bay (edge and tolerance included). No points is never inside.</summary>
        public bool IsInside(Vector2[] localPoints, int count)
        {
            if (count <= 0) return false;
            for (var i = 0; i < count; i++)
            {
                var p = localPoints[i];
                if (Mathf.Abs(p.x) > _half.x + _tolerance || Mathf.Abs(p.y) > _half.y + _tolerance) return false;
            }
            return true;
        }
    }
}
