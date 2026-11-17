using System;
using UnityEngine;

namespace UncannyValet.UI.Pie
{
    /// <summary>Tunables of the pies drawn over a car: the pickup-timer pie (patron waiting) and the wash pie (car in the wash station).</summary>
    [Serializable]
    public sealed class PickupPieSettings
    {
        [SerializeField, Min(8), Tooltip("Diameter of a pie in sprite pixels (32 pixels per world unit).")]
        private int _diameterPixels = 20;
        [SerializeField, Min(4), Tooltip("How many wedge steps the pie is cut into; each step is one pre-built sprite.")]
        private int _steps = 24;
        [SerializeField, Tooltip("Where a lone pie sits relative to the car centre, in world units (up is positive).")]
        private Vector2 _offset = new Vector2(0f, 1.45f);
        [SerializeField, Min(0), Tooltip("Sorting order of the pie; above the car body and its glow.")]
        private int _sortingOrder = 20;
        [SerializeField, Min(0f), Tooltip("Distance between the centres of the pickup pie and the wash pie when a car shows both, in world units. Keep it above the pie diameter (20 px = 0.63).")]
        private float _sideGap = 0.8f;
        [SerializeField, Tooltip("Colour of the wash pie.")]
        private Color _washColor = new Color32(0x2F, 0x8C, 0xF0, 255);

        public int DiameterPixels => Mathf.Max(8, _diameterPixels);
        public int Steps => Mathf.Max(4, _steps);
        public Vector2 Offset => _offset;
        public int SortingOrder => _sortingOrder;
        public float SideGap => Mathf.Max(0f, _sideGap);
        public Color WashColor => _washColor;
    }
}
