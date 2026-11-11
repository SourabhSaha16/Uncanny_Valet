using System;
using UnityEngine;

namespace UncannyValet.Cars.Fx
{
    /// <summary>
    /// Skid-mark numbers of docs/redesign/car_handling_spec.md section 6. Serialized inside CarHandlingConfig, so it needs
    /// no asset of its own. The first four are live (F2 panel); the rest are fixed at build time.
    /// </summary>
    [Serializable]
    public sealed class SkidConfig
    {
        [SerializeField, Min(0.1f), Tooltip("Sideways speed (units/s) above which the car counts as sliding.")]
        private float _slideThreshold = 1.2f;
        [SerializeField, Min(0.1f), Tooltip("Holding the drift button above this speed (units/s) counts as sliding too.")]
        private float _shiftSlideSpeed = 2f;
        [SerializeField, Range(0.05f, 1f), Tooltip("Opacity of the darkest marks (a full drift).")]
        private float _markOpacity = 0.8f;
        [SerializeField, Min(0.5f), Tooltip("Seconds a mark takes to fade out.")]
        private float _markFadeSeconds = 6f;

        [SerializeField, Range(0.1f, 0.95f), Tooltip("A slide ends below this share of its start threshold (stops the sound chattering at the edge).")]
        private float _exitShare = 0.75f;
        [SerializeField, Range(5f, 80f), Tooltip("Slip angle (degrees) that counts as a full drift: darkest marks, loudest squeal.")]
        private float _fullDriftDegrees = 40f;
        [SerializeField, Range(0f, 1f), Tooltip("Opacity of the gentlest slide as a share of the darkest marks.")]
        private float _faintShare = 0.3f;
        [SerializeField, Min(0.1f), Tooltip("Speed (units/s) above which braking counts as hard braking.")]
        private float _hardBrakeSpeed = 4f;

        [SerializeField, Min(0.1f), Tooltip("Distance (units) the tyre travels between two marks. Larger = longer strokes, so the 40 decals last longer.")]
        private float _markSpacing = 1f;
        [SerializeField, Min(0.01f), Tooltip("Width of a mark in world units (one art pixel is 0.07).")]
        private float _markThickness = 0.07f;
        [SerializeField, Range(0f, 0.5f), Tooltip("Rear tyre distance from the car centre line as a share of the car width.")]
        private float _tyreSideShare = 0.38f;
        [SerializeField, Range(0f, 0.5f), Tooltip("Rear tyre distance behind the car centre as a share of the car length.")]
        private float _tyreRearShare = 0.32f;
        [SerializeField, Tooltip("Colour of the marks; their opacity comes from the slide.")]
        private Color _markColor = new Color(0.06f, 0.06f, 0.07f, 1f);
        [SerializeField, Min(1), Tooltip("Most decals alive at once; the oldest is reused. Not live.")]
        private int _decalCap = 40;
        [SerializeField, Tooltip("Sorting order of ground decals: above the asphalt, paint and route arrows, below cars (10).")]
        private int _decalSortingOrder = -3;

        public float SlideThreshold => _slideThreshold;
        public float ShiftSlideSpeed => _shiftSlideSpeed;
        public float MarkOpacity => _markOpacity;
        public float MarkFadeSeconds => _markFadeSeconds;
        public float ExitShare => _exitShare;
        public float FullDriftDegrees => _fullDriftDegrees;
        public float FaintShare => _faintShare;
        public float HardBrakeSpeed => _hardBrakeSpeed;
        public float MarkSpacing => _markSpacing;
        public float MarkThickness => _markThickness;
        public float TyreSideShare => _tyreSideShare;
        public float TyreRearShare => _tyreRearShare;
        public Color MarkColor => _markColor;
        public int DecalCap => _decalCap;
        public int DecalSortingOrder => _decalSortingOrder;
    }
}
