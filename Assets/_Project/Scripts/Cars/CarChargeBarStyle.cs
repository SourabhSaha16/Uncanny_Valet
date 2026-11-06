using System;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Look of the EV charge bar. All lengths in world units.</summary>
    [Serializable]
    public sealed class CarChargeBarStyle
    {
        [SerializeField, Tooltip("Bar thickness (x) and length along the car (y).")]
        private Vector2 _size = new Vector2(0.16f, 1.3f);
        [SerializeField, Min(0f), Tooltip("Dark track visible around the fill.")]
        private float _border = 0.03f;
        [SerializeField, Tooltip("Sideways offset from the car's centre line.")]
        private float _offsetX = 0.3f;
        [SerializeField] private Color _trackColor = new Color(0.05f, 0.06f, 0.1f, 0.9f);
        [SerializeField] private Color _fillColor = new Color(0.1f, 0.6f, 1f, 1f);
        [SerializeField, Tooltip("Fill colour once fully charged.")]
        private Color _fullColor = new Color(0.55f, 1f, 0.95f, 1f);
        [SerializeField, Min(0.1f), Tooltip("How fast the fill eases toward its target, per second.")]
        private float _smoothing = 4f;

        public Vector2 Size => _size;
        public float Border => _border;
        public float OffsetX => _offsetX;
        public Color TrackColor => _trackColor;
        public Color FillColor => _fillColor;
        public Color FullColor => _fullColor;
        public float Smoothing => _smoothing;
    }
}
