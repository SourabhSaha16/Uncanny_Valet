using System;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>Tunables of the tip pop that floats over a patron as they board their car. Read only at runtime.</summary>
    [Serializable]
    public sealed class TipPopSettings
    {
        [SerializeField, Range(0.3f, 3f), Tooltip("Seconds the pop takes to rise and fade out.")]
        private float _durationSeconds = 1.2f;
        [SerializeField, Range(0, 60), Tooltip("Canvas pixels (of 480x270) the pop rises over its life.")]
        private int _risePixels = 18;
        [SerializeField, Range(0f, 3f), Tooltip("World units above the car the pop starts, so it sits over the patron's head.")]
        private float _liftWorldUnits = 0.9f;
        [SerializeField, Range(1, 3), Tooltip("Font scale of a normal tip.")]
        private int _fontScale = 1;
        [SerializeField, Range(1, 4), Tooltip("Font scale of a big tip.")]
        private int _bigFontScale = 2;
        [SerializeField, Min(0), Tooltip("A tip of at least this many dollars gets the big look (gold, larger). 0 turns it off.")]
        private int _bigTipDollars = 40;
        [SerializeField, Range(0f, 0.9f), Tooltip("Fraction of the life the pop stays fully opaque before fading.")]
        private float _holdFraction = 0.4f;
        [SerializeField, Range(1, 8), Tooltip("Most pops alive at once; a new one replaces the oldest.")]
        private int _maxPops = 4;

        public float DurationSeconds => _durationSeconds;
        public int RisePixels => _risePixels;
        public float LiftWorldUnits => _liftWorldUnits;
        public int FontScale => _fontScale;
        public int BigFontScale => _bigFontScale;
        public int BigTipDollars => _bigTipDollars;
        public float HoldFraction => _holdFraction;
        public int MaxPops => _maxPops;
    }
}
