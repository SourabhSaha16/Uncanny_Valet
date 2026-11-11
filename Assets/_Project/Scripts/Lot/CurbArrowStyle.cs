using System;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Look of the painted facing arrows in the pickup zone (world units). The direction comes from the facing rule, not from here.</summary>
    [Serializable]
    public sealed class CurbArrowStyle
    {
        [SerializeField, Min(0.5f), Tooltip("Length of each arrow, tail to tip.")] private float _length = 3f;
        [SerializeField, Min(0.05f), Tooltip("Shaft and head stroke thickness.")] private float _thickness = 0.16f;
        [SerializeField, Min(0.1f), Tooltip("Length of each head stroke.")] private float _head = 0.5f;
        [SerializeField, Range(10f, 80f), Tooltip("Angle of the head strokes from the shaft.")] private float _headDegrees = 40f;
        [SerializeField, Min(0f), Tooltip("Dark outline grown around every stroke so the arrow reads on the cobbles.")] private float _outline = 0.06f;
        [SerializeField, Min(0f), Tooltip("Gap kept between an arrow and the zone's painted edge.")] private float _edgeInset = 0.2f;
        [SerializeField, Tooltip("No car in the zone.")] private Color _idleColor = new Color(0.97f, 0.97f, 0.94f);
        [SerializeField, Tooltip("A car is in the zone but faces the wrong way.")] private Color _wrongColor = new Color(0.98f, 0.82f, 0.2f);
        [SerializeField, Tooltip("A car in the zone faces as required.")] private Color _rightColor = new Color(0.3f, 0.85f, 0.4f);
        [SerializeField] private Color _outlineColor = new Color(0.1f, 0.1f, 0.12f, 0.9f);

        public float Length => _length;
        public float Thickness => _thickness;
        public float Head => _head;
        public float HeadDegrees => _headDegrees;
        public float Outline => _outline;
        public float EdgeInset => _edgeInset;
        public Color OutlineColor => _outlineColor;

        /// <summary>The fill colour for a state.</summary>
        public Color ColorOf(CurbArrowState state)
        {
            if (state == CurbArrowState.Right) return _rightColor;
            return state == CurbArrowState.Wrong ? _wrongColor : _idleColor;
        }
    }
}
