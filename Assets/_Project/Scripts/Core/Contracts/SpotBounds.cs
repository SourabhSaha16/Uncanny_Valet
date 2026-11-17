using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>The inner rectangle of a painted bay in world units.</summary>
    public readonly struct SpotBounds
    {
        /// <summary>World position of the bay centre.</summary>
        public readonly Vector2 Center;
        /// <summary>Width (x) and length (y) of the bay before rotation, in world units.</summary>
        public readonly Vector2 Size;
        /// <summary>Rotation of the bay around its centre, in degrees (0 = length along up).</summary>
        public readonly float RotationDegrees;

        public SpotBounds(Vector2 center, Vector2 size, float rotationDegrees)
        {
            Center = center;
            Size = size;
            RotationDegrees = rotationDegrees;
        }
    }
}
