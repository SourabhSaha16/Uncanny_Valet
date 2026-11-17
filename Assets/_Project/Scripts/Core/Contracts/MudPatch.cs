using System;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>One mud puddle: a round patch centred on a point. The size is the diameter in world units (2.5 to 4 looks right).</summary>
    [Serializable]
    public struct MudPatch
    {
        [SerializeField, Tooltip("Centre of the puddle, in the lot's own space (the lot object's origin).")]
        private Vector2 _center;
        [SerializeField, Min(0.5f), Tooltip("Width of the puddle in world units. About 2.5 to 4.")]
        private float _size;

        public MudPatch(Vector2 center, float size)
        {
            _center = center;
            _size = size;
        }

        public Vector2 Center => _center;
        public float Size => _size;

        /// <summary>True when <paramref name="point"/> is inside the puddle; the rim counts as inside.</summary>
        public bool Contains(Vector2 point)
        {
            var radius = _size * 0.5f;
            return (point - _center).sqrMagnitude <= radius * radius;
        }

        /// <summary>The same puddle moved by <paramref name="offset"/> (lot space to world space).</summary>
        public MudPatch Moved(Vector2 offset) => new MudPatch(_center + offset, _size);
    }
}
