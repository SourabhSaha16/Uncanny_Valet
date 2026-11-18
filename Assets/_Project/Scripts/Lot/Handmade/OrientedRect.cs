using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>A rectangle turned by <see cref="RotationDegrees"/> (counter-clockwise, like a transform's z) around its centre. Width runs along the local x axis, length along local y.</summary>
    public readonly struct OrientedRect
    {
        private const float Epsilon = 0.0001f;

        public readonly Vector2 Center;
        public readonly Vector2 Size;
        public readonly float RotationDegrees;

        public OrientedRect(Vector2 center, Vector2 size, float rotationDegrees)
        {
            Center = center;
            Size = size;
            RotationDegrees = rotationDegrees;
        }

        /// <summary>The unit vector of the local x axis in world space.</summary>
        public Vector2 Right => Axis(0f);

        /// <summary>The unit vector of the local y axis in world space (the way a bay's length runs).</summary>
        public Vector2 Up => Axis(90f);

        /// <summary>Corner <paramref name="index"/> (0 to 3, going round).</summary>
        public Vector2 Corner(int index)
        {
            var sx = (index == 1 || index == 2) ? 0.5f : -0.5f;
            var sy = index >= 2 ? 0.5f : -0.5f;
            return Center + Right * (Size.x * sx) + Up * (Size.y * sy);
        }

        /// <summary>The point in the rectangle's own frame (x along width, y along length, origin at the centre).</summary>
        public Vector2 ToLocal(Vector2 world)
        {
            var d = world - Center;
            return new Vector2(Vector2.Dot(d, Right), Vector2.Dot(d, Up));
        }

        /// <summary>True when the point lies inside, the edge included.</summary>
        public bool Contains(Vector2 world)
        {
            var p = ToLocal(world);
            return Mathf.Abs(p.x) <= Size.x * 0.5f + Epsilon && Mathf.Abs(p.y) <= Size.y * 0.5f + Epsilon;
        }

        /// <summary>True when the two rectangles share area (touching edges do not count). Separating axis test.</summary>
        public bool Overlaps(OrientedRect other)
        {
            return !Separated(this, other, Right) && !Separated(this, other, Up)
                && !Separated(this, other, other.Right) && !Separated(this, other, other.Up);
        }

        /// <summary>True when the segment from <paramref name="a"/> to <paramref name="b"/> passes through the rectangle (grazing an edge does not count).</summary>
        public bool IntersectsSegment(Vector2 a, Vector2 b)
        {
            var from = ToLocal(a);
            var to = ToLocal(b);
            var t0 = 0f;
            var t1 = 1f;
            return Clip(from.x, to.x - from.x, Size.x * 0.5f, ref t0, ref t1)
                && Clip(from.y, to.y - from.y, Size.y * 0.5f, ref t0, ref t1);
        }

        /// <summary>The shortest gap between the two rectangles; 0 when they touch or overlap.</summary>
        public float Distance(OrientedRect other)
        {
            if (Overlaps(other)) return 0f;
            var best = float.MaxValue;
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                {
                    best = Mathf.Min(best, RouteMath.DistanceToSegment(Corner(i), other.Corner(j), other.Corner((j + 1) % 4)));
                    best = Mathf.Min(best, RouteMath.DistanceToSegment(other.Corner(i), Corner(j), Corner((j + 1) % 4)));
                }
            return best;
        }

        private Vector2 Axis(float extraDegrees)
        {
            var rad = (RotationDegrees + extraDegrees) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        private static bool Separated(OrientedRect a, OrientedRect b, Vector2 axis)
        {
            Project(a, axis, out var aMin, out var aMax);
            Project(b, axis, out var bMin, out var bMax);
            return aMax <= bMin + Epsilon || bMax <= aMin + Epsilon;
        }

        private static void Project(OrientedRect r, Vector2 axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;
            for (var i = 0; i < 4; i++)
            {
                var d = Vector2.Dot(r.Corner(i), axis);
                min = Mathf.Min(min, d);
                max = Mathf.Max(max, d);
            }
        }

        // Liang-Barsky against the slab -half..half on one axis, shrunk by epsilon so grazing an edge is not a hit.
        private static bool Clip(float start, float delta, float half, ref float t0, ref float t1)
        {
            half -= Epsilon;
            if (Mathf.Abs(delta) < Epsilon) return Mathf.Abs(start) < half;
            var enter = (-half - start) / delta;
            var exit = (half - start) / delta;
            if (enter > exit) (enter, exit) = (exit, enter);
            t0 = Mathf.Max(t0, enter);
            t1 = Mathf.Min(t1, exit);
            return t0 < t1;
        }
    }
}
