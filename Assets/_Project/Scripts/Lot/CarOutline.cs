using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Reads a car's collider outline as points in a bay's local space (allocation free).</summary>
    public sealed class CarOutline
    {
        private const int CornerCount = 4;
        private readonly Vector2[] _points = new Vector2[CornerCount];

        public Vector2[] Points => _points;

        /// <summary>Fills the corners of the car's box collider (or the bounds of any other collider); returns the count, 0 if none.</summary>
        public int Read(Transform bay, Transform car)
        {
            var body = car.GetComponentInChildren<Collider2D>();
            if (body == null) return 0;
            if (body is BoxCollider2D box)
            {
                var h = box.size * 0.5f;
                Corner(0, bay, box.transform, box.offset + new Vector2(-h.x, -h.y));
                Corner(1, bay, box.transform, box.offset + new Vector2(h.x, -h.y));
                Corner(2, bay, box.transform, box.offset + new Vector2(h.x, h.y));
                Corner(3, bay, box.transform, box.offset + new Vector2(-h.x, h.y));
                return CornerCount;
            }
            var b = body.bounds;
            _points[0] = bay.InverseTransformPoint(b.min);
            _points[1] = bay.InverseTransformPoint(new Vector3(b.max.x, b.min.y));
            _points[2] = bay.InverseTransformPoint(b.max);
            _points[3] = bay.InverseTransformPoint(new Vector3(b.min.x, b.max.y));
            return CornerCount;
        }

        private void Corner(int i, Transform bay, Transform owner, Vector2 local) =>
            _points[i] = bay.InverseTransformPoint(owner.TransformPoint(local));
    }
}
