using System;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Size, placement and colour of the two front side mirrors. All lengths in world units.</summary>
    [Serializable]
    public sealed class CarMirrorStyle
    {
        [SerializeField, Tooltip("Mirror width (x) and depth (y).")]
        private Vector2 _size = new Vector2(0.18f, 0.22f);
        [SerializeField, Min(0f), Tooltip("Distance from the front edge of the body to the front edge of the mirror.")]
        private float _frontInset = 0.35f;
        [SerializeField, Min(0.01f), Tooltip("How far the mirror sticks out past the side of the body.")]
        private float _outset = 0.12f;
        [SerializeField] private Color _color = new Color(0.25f, 0.25f, 0.27f, 1f);

        /// <summary>Mirror width (x) and depth (y).</summary>
        public Vector2 Size => _size;
        /// <summary>Gap between the car's front edge and the mirror's front edge.</summary>
        public float FrontInset => _frontInset;
        /// <summary>How far the mirror sticks out past the body side.</summary>
        public float Outset => _outset;
        public Color Color => _color;
    }
}
