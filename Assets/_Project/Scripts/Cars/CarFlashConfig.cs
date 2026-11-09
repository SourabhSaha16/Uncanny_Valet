using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Tunables for the car's per-ticket body tint and the pickup flash: how strongly the tag colour tints the
    /// sprite, how fast the pulse blinks as the patron's patience runs out, and the alarm colours. Read only at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Car Flash Config", fileName = "CarFlashConfig")]
    public sealed class CarFlashConfig : ScriptableObject
    {
        [Header("Body tint")]
        [SerializeField, Range(0f, 1f), Tooltip("How far the tag colour is lerped toward white, so outlines and windows stay readable.")]
        private float _tintSoftness = 0.45f;
        [SerializeField, Range(0f, 1f), Tooltip("Fraction of the softened tag tint an EV gets (0 keeps the EV sprite's own look).")]
        private float _evTintStrength = 0.25f;
        [SerializeField, Range(0f, 1f), Tooltip("How far the body tint moves toward the flash colour while the pulse is on.")]
        private float _pulseMix = 0.65f;

        [Header("Flash speed")]
        [SerializeField, Min(0.1f), Tooltip("Flashes per second right after the pickup is requested.")]
        private float _minHz = 1f;
        [SerializeField, Min(0.1f), Tooltip("Flashes per second when patience is gone.")]
        private float _maxHz = 6f;

        [Header("Alarm levels (fraction of the pickup patience that has run out)")]
        [SerializeField, Range(0f, 1f), Tooltip("From here the flash turns amber.")]
        private float _amberFrom = 0.6f;
        [SerializeField, Range(0f, 1f), Tooltip("From here (or when expired) the flash turns orange-red.")]
        private float _dangerFrom = 0.85f;

        [Header("Colours")]
        [SerializeField] private Color _calmColor = new Color32(0x38, 0xD6, 0xF0, 255);
        [SerializeField] private Color _amberColor = new Color32(0xFF, 0xB0, 0x20, 255);
        [SerializeField] private Color _dangerColor = new Color32(0xFF, 0x5A, 0x2A, 255);

        /// <summary>Share of white mixed into the tag colour for the car body.</summary>
        public float TintSoftness => _tintSoftness;
        /// <summary>Share of the softened tint an EV receives.</summary>
        public float EvTintStrength => _evTintStrength;
        /// <summary>Share of the flash colour mixed into the body while the pulse is on.</summary>
        public float PulseMix => _pulseMix;
        public float MinHz => _minHz;
        public float MaxHz => Mathf.Max(_minHz, _maxHz);
        public float AmberFrom => _amberFrom;
        public float DangerFrom => Mathf.Max(_amberFrom, _dangerFrom);

        /// <summary>The flash colour of an alarm level.</summary>
        public Color ColorFor(FlashLevel level)
        {
            if (level == FlashLevel.Danger) return _dangerColor;
            return level == FlashLevel.Amber ? _amberColor : _calmColor;
        }
    }
}
