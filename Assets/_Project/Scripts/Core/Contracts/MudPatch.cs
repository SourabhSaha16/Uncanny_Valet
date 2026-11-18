using System;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// One mud patch centred on a point. By default a round puddle whose size is the diameter in world units (2.5 to 4 looks right); with a
    /// height above zero it is a rectangle <c>size</c> wide and <c>height</c> tall (a field).
    /// </summary>
    [Serializable]
    public struct MudPatch
    {
        [SerializeField, Tooltip("Centre of the puddle, in the lot's own space (the lot object's origin).")]
        private Vector2 _center;
        [SerializeField, Min(0.5f), Tooltip("Width of the puddle in world units. About 2.5 to 4.")]
        private float _size;
        [SerializeField, Min(0f), Tooltip("Height of a rectangular patch in world units. 0 keeps the patch a round puddle (the width is then its diameter).")]
        private float _height;

        public MudPatch(Vector2 center, float size) : this(center, size, 0f)
        {
        }

        /// <summary>A rectangle <paramref name="width"/> wide and <paramref name="height"/> tall; a height of 0 makes a round puddle.</summary>
        public MudPatch(Vector2 center, float width, float height)
        {
            _center = center;
            _size = width;
            _height = height;
        }

        public Vector2 Center => _center;
        public float Size => _size;
        /// <summary>Height of a rectangular patch; 0 for a round puddle.</summary>
        public float Height => _height;
        /// <summary>True when the patch is a rectangle rather than a round puddle.</summary>
        public bool IsRectangle => _height > 0f;
        /// <summary>Extent of the patch along y: the height of a rectangle, the diameter of a puddle.</summary>
        public float ExtentY => IsRectangle ? _height : _size;

        /// <summary>True when <paramref name="point"/> is inside the patch; the rim counts as inside.</summary>
        public bool Contains(Vector2 point)
        {
            if (IsRectangle) return Mathf.Abs(point.x - _center.x) <= _size * 0.5f && Mathf.Abs(point.y - _center.y) <= _height * 0.5f;
            var radius = _size * 0.5f;
            return (point - _center).sqrMagnitude <= radius * radius;
        }

        /// <summary>The same puddle moved by <paramref name="offset"/> (lot space to world space).</summary>
        public MudPatch Moved(Vector2 offset) => new MudPatch(_center + offset, _size, _height);
    }
}
