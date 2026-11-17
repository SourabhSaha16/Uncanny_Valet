using System;
using UnityEngine;

namespace UncannyValet.Core.Art
{
    /// <summary>One 2D point light of a <see cref="LightingPreset"/>: where, how far, what colour and an optional flicker.</summary>
    [Serializable]
    public struct PointLightSpec
    {
        [SerializeField, Tooltip("Centre of the light in the lot's own space.")] private Vector2 _position;
        [SerializeField, Min(0.1f), Tooltip("How far the light reaches, in world units.")] private float _radius;
        [SerializeField] private Color _colour;
        [SerializeField, Min(0f)] private float _intensity;
        [SerializeField, Range(0f, 1f), Tooltip("How much the light dims at the lowest point of its flicker (0 = steady).")]
        private float _flicker;
        [SerializeField, Min(0f), Tooltip("Flicker speed in changes per second.")] private float _flickerSpeed;

        public PointLightSpec(Vector2 position, float radius, Color colour, float intensity, float flicker = 0f, float flickerSpeed = 0f)
        {
            _position = position;
            _radius = radius;
            _colour = colour;
            _intensity = intensity;
            _flicker = flicker;
            _flickerSpeed = flickerSpeed;
        }

        public Vector2 Position => _position;
        public float Radius => _radius;
        public Color Colour => _colour;
        public float Intensity => _intensity;
        public float Flicker => _flicker;
        public float FlickerSpeed => _flickerSpeed;
    }
}
