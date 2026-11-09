using System;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>One top-down car sprite plus the measurements needed to fit it to a car. Filled by the editor builder.</summary>
    [Serializable]
    public sealed class CarSpriteEntry
    {
        [SerializeField] private Sprite _sprite;
        [SerializeField, Tooltip("Size in world units of the opaque pixels inside the sprite cell.")]
        private Vector2 _contentSize = Vector2.one;
        [SerializeField, Tooltip("Centre of the opaque pixels relative to the sprite pivot, world units.")]
        private Vector2 _contentCenter;
        [SerializeField, Tooltip("Extra rotation so the sprite nose points +Y. Use 0 or 180 only.")]
        private float _rotationOffsetDegrees;

        public Sprite Sprite => _sprite;
        public Vector2 ContentSize => _contentSize;
        public Vector2 ContentCenter => _contentCenter;
        public float RotationOffsetDegrees => _rotationOffsetDegrees;
    }
}
