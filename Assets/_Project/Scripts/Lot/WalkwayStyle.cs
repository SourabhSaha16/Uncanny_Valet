using System;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Tunables for the crosswalk zebra bars, the curb and arrival direction marks and the curb standing spots. World units.</summary>
    [Serializable]
    public sealed class WalkwayStyle
    {
        [Header("Crosswalk zebra bars")]
        [SerializeField, Min(0.05f), Tooltip("Length of one zebra bar along the walking direction.")]
        private float _zebraBar = 0.3f;
        [SerializeField, Min(0.05f), Tooltip("Gap between two zebra bars.")]
        private float _zebraGap = 0.3f;
        [SerializeField, Min(0f), Tooltip("How far each zebra bar stops short of the crosswalk edge, on both sides.")]
        private float _zebraMargin = 0.05f;
        [Header("Arrow heads")]
        [SerializeField, Range(10f, 80f)] private float _arrowHeadDegrees = 35f;
        [Header("Leaving route arrows (curb to the road)")]
        [SerializeField, Min(0.2f), Tooltip("Length of one painted arrow on the route a delivered car drives.")]
        private float _routeArrowLength = 0.9f;
        [SerializeField, Min(0.02f), Tooltip("Stroke width of a route arrow.")]
        private float _routeArrowThickness = 0.1f;
        [SerializeField, Min(0.05f), Tooltip("Length of the two head strokes of a route arrow.")]
        private float _routeArrowHead = 0.3f;
        [SerializeField, Min(0.5f), Tooltip("Distance along the route between two arrows.")]
        private float _routeArrowSpacing = 2.4f;
        [SerializeField, Min(0f), Tooltip("Distance along the route from the curb zone to the first arrow.")]
        private float _routeArrowStart = 2.4f;
        [SerializeField, Min(0f), Tooltip("Dark outline added around every stroke of a route arrow so it reads on any ground.")]
        private float _markOutline = 0.05f;
        [Header("Arrival queue slots (dotted boxes)")]
        [SerializeField, Min(0.02f), Tooltip("Line thickness of the dotted box around each arrival slot.")]
        private float _slotLine = 0.1f;
        [SerializeField, Min(0.05f), Tooltip("Length of one dash of the dotted slot box.")]
        private float _slotDash = 0.4f;
        [SerializeField, Min(0.02f), Tooltip("Gap between two dashes of the dotted slot box.")]
        private float _slotGap = 0.25f;
        [SerializeField, Min(0f), Tooltip("How far the dotted box sits inside the slot's car footprint, so neighbouring boxes do not touch.")]
        private float _slotInset = 0.12f;
        [SerializeField] private Color _markColor = new Color(1f, 1f, 0.96f, 1f);
        [SerializeField] private Color _markOutlineColor = new Color(0.1f, 0.09f, 0.15f, 1f);
        [Header("Curb standing spots")]
        [SerializeField, Min(0.1f), Tooltip("Distance of the curb standing spots from the curb zone's east edge.")]
        private float _waitInset = 0.9f;
        [SerializeField, Min(0.1f), Tooltip("Spacing between curb standing spots along the curb.")]
        private float _waitSpacing = 1.6f;
        [SerializeField, Range(1, 5), Tooltip("Number of curb standing spots.")]
        private int _waitSlots = 3;

        public float ZebraBar => _zebraBar;
        public float ZebraGap => _zebraGap;
        public float ZebraMargin => _zebraMargin;
        public float ArrowHeadDegrees => _arrowHeadDegrees;
        public float RouteArrowLength => _routeArrowLength;
        public float RouteArrowThickness => _routeArrowThickness;
        public float RouteArrowHead => _routeArrowHead;
        public float RouteArrowSpacing => _routeArrowSpacing;
        public float RouteArrowStart => _routeArrowStart;
        public float MarkOutline => _markOutline;
        public float SlotLine => _slotLine;
        public float SlotDash => _slotDash;
        public float SlotGap => _slotGap;
        public float SlotInset => _slotInset;
        public Color MarkColor => _markColor;
        public Color MarkOutlineColor => _markOutlineColor;
        public float WaitInset => _waitInset;
        public float WaitSpacing => _waitSpacing;
        public int WaitSlots => _waitSlots;
    }
}
