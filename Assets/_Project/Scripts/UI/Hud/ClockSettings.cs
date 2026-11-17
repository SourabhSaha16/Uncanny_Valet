using System;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>Tunables of the shift clock tab, which hangs directly under the blessing (score) panel at the left screen edge.</summary>
    [Serializable]
    public sealed class ClockSettings
    {
        /// <summary>Default gap between the score panel and the clock tab, in canvas pixels.</summary>
        public const int DefaultGapPixels = 2;

        [SerializeField, Range(0, 20), Tooltip("Canvas pixels (of 480x270) between the bottom of the score panel and the top of the clock tab.")]
        private int _gapPixels = DefaultGapPixels;
        [SerializeField, Range(0, 40), Tooltip("Extra canvas pixels the clock tab is pushed right of the left screen margin.")]
        private int _insetPixels;

        public int GapPixels => Mathf.Clamp(_gapPixels, 0, 20);
        public int InsetPixels => Mathf.Clamp(_insetPixels, 0, 40);
    }
}
