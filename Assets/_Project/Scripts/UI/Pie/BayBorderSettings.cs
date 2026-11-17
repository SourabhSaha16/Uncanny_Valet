using System;
using UnityEngine;

namespace UncannyValet.UI.Pie
{
    /// <summary>Tunables of the bay border flashes: the green flash on a clean park and the pickup-timer flash while a patron waits.</summary>
    [Serializable]
    public sealed class BayBorderSettings
    {
        [SerializeField, Min(0.1f), Tooltip("How long the border flashes green after a car parks completely inside its bay, in seconds.")]
        private float _parkedFlashSeconds = 0.8f;
        [SerializeField, Min(0.5f), Tooltip("Blinks per second of the green flash.")]
        private float _parkedFlashHz = 5f;
        [SerializeField, Min(0.5f), Tooltip("Blinks per second while the patron is happy (green pickup timer).")]
        private float _happyHz = 1.5f;
        [SerializeField, Min(0.5f), Tooltip("Blinks per second while the patron is impatient (yellow pickup timer).")]
        private float _impatientHz = 3f;
        [SerializeField, Min(0.5f), Tooltip("Blinks per second while the patron is angry (red pickup timer).")]
        private float _angryHz = 7f;
        [SerializeField, Range(1, 4), Tooltip("Thickness of the border line in sprite pixels (32 pixels per world unit).")]
        private int _thicknessPixels = 2;
        [SerializeField, Tooltip("Sorting order of the border: above the bay floor art, below cars (car bodies sit at 10).")]
        private int _sortingOrder = 5;

        public float ParkedFlashSeconds => Mathf.Max(0.1f, _parkedFlashSeconds);
        public float ParkedFlashHz => Mathf.Max(0.5f, _parkedFlashHz);
        public float HappyHz => Mathf.Max(0.5f, _happyHz);
        public float ImpatientHz => Mathf.Max(0.5f, _impatientHz);
        public float AngryHz => Mathf.Max(0.5f, _angryHz);
        public int ThicknessPixels => Mathf.Clamp(_thicknessPixels, 1, 4);
        public int SortingOrder => _sortingOrder;
    }
}
