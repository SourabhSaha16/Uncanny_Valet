using System;
using UnityEngine;

namespace UncannyValet.UI.Pie
{
    /// <summary>Tunables of the pickup-timer pie drawn over a car whose patron is waiting.</summary>
    [Serializable]
    public sealed class PickupPieSettings
    {
        [SerializeField, Min(8), Tooltip("Diameter of the pie in sprite pixels (32 pixels per world unit).")]
        private int _diameterPixels = 20;
        [SerializeField, Min(4), Tooltip("How many wedge steps the pie is cut into; each step is one pre-built sprite.")]
        private int _steps = 24;
        [SerializeField, Tooltip("Where the pie sits relative to the car centre, in world units (up is positive).")]
        private Vector2 _offset = new Vector2(0f, 1.45f);
        [SerializeField, Min(0), Tooltip("Sorting order of the pie; above the car body and its glow.")]
        private int _sortingOrder = 20;

        public int DiameterPixels => Mathf.Max(8, _diameterPixels);
        public int Steps => Mathf.Max(4, _steps);
        public Vector2 Offset => _offset;
        public int SortingOrder => _sortingOrder;
    }
}
