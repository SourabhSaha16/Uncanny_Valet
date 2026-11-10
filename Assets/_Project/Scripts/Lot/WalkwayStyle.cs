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
        [SerializeField, Min(0f), Tooltip("How far the crossing between the first bay columns walks past the aisle edge. 0 keeps walkers out of the gap beside the EV bays and the wash slot, which is narrower than a person.")]
        private float _gapEntry;
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
        [Header("Curb direction marking")]
        [SerializeField, Min(0.5f), Tooltip("Length of the down arrows painted on the curb zone.")]
        private float _curbArrowLength = 1.6f;
        [SerializeField, Range(0.1f, 0.45f), Tooltip("Distance of each curb arrow from the zone centre, as a share of the zone width.")]
        private float _curbArrowSideShare = 0.3f;
        [SerializeField, Min(0f), Tooltip("Gap between a curb arrow tip and the chevron under it.")]
        private float _curbChevronGap = 0.15f;
        [Header("Big direction marks (curb and arrival slots)")]
        [SerializeField, Min(0.1f), Tooltip("Stroke width of the big curb and arrival arrows.")]
        private float _markThickness = 0.16f;
        [SerializeField, Min(0.1f), Tooltip("Length of the head and chevron strokes of the big arrows.")]
        private float _markHeadLength = 0.45f;
        [SerializeField, Min(0f), Tooltip("Dark outline added around every stroke of the big arrows so they read on any ground.")]
        private float _markOutline = 0.05f;
        [SerializeField, Min(0.5f), Tooltip("Length of the up arrows painted at each arrival slot.")]
        private float _arrivalArrowLength = 1.4f;
        [SerializeField] private Color _markColor = new Color(1f, 1f, 0.96f, 1f);
        [SerializeField] private Color _markOutlineColor = new Color(0.1f, 0.09f, 0.15f, 1f);
        [Header("Standing spots and doorway")]
        [SerializeField, Min(0.1f), Tooltip("Distance of the curb standing spots from the curb zone's east edge.")]
        private float _waitInset = 0.9f;
        [SerializeField, Min(0.1f), Tooltip("Spacing between curb standing spots along the curb.")]
        private float _waitSpacing = 1.6f;
        [SerializeField, Range(1, 5), Tooltip("Number of curb standing spots.")]
        private int _waitSlots = 3;
        [SerializeField, Min(0f), Tooltip("How far past the sidewalk centre line the door sits, towards the diner.")]
        private float _doorDepth = 0.9f;
        [SerializeField, Min(0f), Tooltip("How far beyond the lot sidewalks the road routes start, so walkers appear off-screen.")]
        private float _offscreenReach = 3f;
        [SerializeField] private Color _arrowColor = new Color(0.97f, 0.97f, 0.94f, 0.9f);

        public int CrossingBars => _crossingBars;
        public float BarWidth => _barWidth;
        public float BarGap => _barGap;
        public float BayEntry => _bayEntry;
        public float GapEntry => _gapEntry;
        public float RoadReach => _roadReach;
        public float NorthMargin => _northMargin;
        public float ArrowLength => _arrowLength;
        public float ArrowThickness => _arrowThickness;
        public float ArrowHeadLength => _arrowHeadLength;
        public float ArrowHeadDegrees => _arrowHeadDegrees;
        public float LaneInset => _laneInset;
        public float CurbArrowLength => _curbArrowLength;
        public float CurbArrowSideShare => _curbArrowSideShare;
        public float CurbChevronGap => _curbChevronGap;
        public float MarkThickness => _markThickness;
        public float MarkHeadLength => _markHeadLength;
        public float MarkOutline => _markOutline;
        public float ArrivalArrowLength => _arrivalArrowLength;
        public Color MarkColor => _markColor;
        public Color MarkOutlineColor => _markOutlineColor;
        public float WaitInset => _waitInset;
        public float WaitSpacing => _waitSpacing;
        public int WaitSlots => _waitSlots;
        public float DoorDepth => _doorDepth;
        public float OffscreenReach => _offscreenReach;
        public Color ArrowColor => _arrowColor;

        /// <summary>Width of one crossing, from the first stripe's edge to the last.</summary>
        public float CrossingWidth => _crossingBars * _barWidth + (_crossingBars - 1) * _barGap;
    }
}
