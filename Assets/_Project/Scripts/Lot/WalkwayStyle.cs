using System;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Tunables for the painted walkway bands, the curb and arrival direction marks and the curb standing spots. World units.</summary>
    [Serializable]
    public sealed class WalkwayStyle
    {
        [Header("Walkway bands")]
        [SerializeField, Min(0.02f), Tooltip("Thickness of the painted edge lines of a walkway band.")]
        private float _bandEdge = 0.12f;
        [SerializeField, Range(0f, 1f), Tooltip("Opacity of the band fill (the crosswalk colour over the asphalt).")]
        private float _bandFillAlpha = 0.22f;
        [Header("Arrow heads")]
        [SerializeField, Range(10f, 80f)] private float _arrowHeadDegrees = 35f;
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
        [Header("Curb standing spots")]
        [SerializeField, Min(0.1f), Tooltip("Distance of the curb standing spots from the curb zone's east edge.")]
        private float _waitInset = 0.9f;
        [SerializeField, Min(0.1f), Tooltip("Spacing between curb standing spots along the curb.")]
        private float _waitSpacing = 1.6f;
        [SerializeField, Range(1, 5), Tooltip("Number of curb standing spots.")]
        private int _waitSlots = 3;

        public float BandEdge => _bandEdge;
        public float BandFillAlpha => _bandFillAlpha;
        public float ArrowHeadDegrees => _arrowHeadDegrees;
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
    }
}
