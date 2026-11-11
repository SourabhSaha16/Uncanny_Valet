using UnityEngine;

namespace UncannyValet.Cars.Fx
{
    /// <summary>One straight skid stroke: where it sits, how it is rotated (local up along the stroke) and how long it is.</summary>
    public readonly struct MarkSegment
    {
        public readonly Vector2 Center;
        public readonly float AngleDegrees;
        public readonly float Length;

        public MarkSegment(Vector2 center, float angleDegrees, float length)
        {
            Center = center;
            AngleDegrees = angleDegrees;
            Length = length;
        }
    }
}
