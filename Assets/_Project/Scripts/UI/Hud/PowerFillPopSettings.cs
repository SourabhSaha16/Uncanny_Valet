using System;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>Tunables of the "+X" bolt pop that floats beside the power bar when the bar gains. Read only at runtime.</summary>
    [Serializable]
    public sealed class PowerFillPopSettings
    {
        [SerializeField, Range(0.3f, 3f), Tooltip("Seconds a pop takes to rise and fade out.")]
        private float _durationSeconds = 1.1f;
        [SerializeField, Range(0, 60), Tooltip("Canvas pixels (of 480x270) a pop rises over its life.")]
        private int _risePixels = 16;
        [SerializeField, Range(0f, 0.9f), Tooltip("Fraction of the life a pop stays fully opaque before fading.")]
        private float _holdFraction = 0.35f;
        [SerializeField, Range(1, 3), Tooltip("Font scale of the +X text.")]
        private int _fontScale = 1;
        [SerializeField, Range(8, 32), Tooltip("Canvas pixels between stacked pops, so pops that arrive together do not overlap.")]
        private int _slotSpacingPixels = 13;
        [SerializeField, Range(0.05f, 2f), Tooltip("Seconds a new pop holds its stack slot; a pop arriving inside it takes the next free slot below.")]
        private float _slotHoldSeconds = 0.5f;
        [SerializeField, Range(2, 8), Tooltip("Most pops alive at once (and stack slots); a new one replaces the oldest.")]
        private int _maxPops = 4;
        [SerializeField, Range(0, 40), Tooltip("Canvas pixels between the pop and the power icons on its right.")]
        private int _gapPixels = 4;
        [SerializeField, Range(0f, 1f), Tooltip("How far the source colour is pulled toward gold; 1 makes every pop gold, 0 uses each source's own colour.")]
        private float _goldPull = 0.5f;

        public float DurationSeconds => _durationSeconds;
        public int RisePixels => _risePixels;
        public float HoldFraction => _holdFraction;
        public int FontScale => _fontScale;
        public int SlotSpacingPixels => _slotSpacingPixels;
        public float SlotHoldSeconds => _slotHoldSeconds;
        public int MaxPops => _maxPops;
        public int GapPixels => _gapPixels;
        public float GoldPull => _goldPull;
    }
}
