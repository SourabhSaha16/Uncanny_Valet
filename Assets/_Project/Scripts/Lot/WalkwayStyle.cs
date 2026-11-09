using System;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Tunables for the zebra crossings, painted arrows and pedestrian routes inside and around the lot. World units.</summary>
    [Serializable]
    public sealed class WalkwayStyle
    {
        [SerializeField, Min(1), Tooltip("Stripes in a crossing inside the lot.")]
        private int _crossingBars = 3;
        [SerializeField, Min(0.1f)] private float _barWidth = 0.45f;
        [SerializeField, Min(0f)] private float _barGap = 0.45f;
        [SerializeField, Min(0f), Tooltip("How far a crossing route walks past the aisle edge into the bays or the open strip.")]
        private float _bayEntry = 1.5f;
        [SerializeField, Min(1f), Tooltip("How far the road routes walk along the sidewalks either side of the road crossing.")]
        private float _roadReach = 7f;
        [SerializeField, Min(0f), Tooltip("Margin kept at both ends of the north sidewalk route.")]
        private float _northMargin = 2f;
        [Header("Aisle arrows")]
        [SerializeField, Min(0.5f)] private float _arrowLength = 1.5f;
        [SerializeField, Min(0.05f)] private float _arrowThickness = 0.2f;
        [SerializeField, Min(0.1f)] private float _arrowHeadLength = 0.6f;
        [SerializeField, Range(10f, 80f)] private float _arrowHeadDegrees = 35f;
        [SerializeField, Min(0f), Tooltip("Distance of the arrow lanes from the middle of the aisle.")]
        private float _laneInset = 1.25f;
        [SerializeField] private Color _arrowColor = new Color(0.97f, 0.97f, 0.94f, 0.9f);

        public int CrossingBars => _crossingBars;
        public float BarWidth => _barWidth;
        public float BarGap => _barGap;
        public float BayEntry => _bayEntry;
        public float RoadReach => _roadReach;
        public float NorthMargin => _northMargin;
        public float ArrowLength => _arrowLength;
        public float ArrowThickness => _arrowThickness;
        public float ArrowHeadLength => _arrowHeadLength;
        public float ArrowHeadDegrees => _arrowHeadDegrees;
        public float LaneInset => _laneInset;
        public Color ArrowColor => _arrowColor;

        /// <summary>Width of one crossing, from the first stripe's edge to the last.</summary>
        public float CrossingWidth => _crossingBars * _barWidth + (_crossingBars - 1) * _barGap;
    }
}
