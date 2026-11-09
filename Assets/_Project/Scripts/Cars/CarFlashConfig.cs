using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Tunables for the pickup flash on a car: how fast it blinks as the patron's patience runs out, the alarm
    /// colours, and the size of the key tag and the outline. Read only at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Car Flash Config", fileName = "CarFlashConfig")]
    public sealed class CarFlashConfig : ScriptableObject
    {
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
        [SerializeField] private Color _tagBackingColor = new Color32(0x1A, 0x16, 0x26, 255);

        [Header("Shapes (world units)")]
        [SerializeField, Tooltip("Width across the car and length along it of the key-tag stripe.")]
        private Vector2 _tagSize = new Vector2(0.9f, 0.34f);
        [SerializeField, Min(0f), Tooltip("Dark border around the key-tag stripe.")]
        private float _tagBorder = 0.08f;
        [SerializeField, Min(0f), Tooltip("How far the flashing outline reaches past the car body.")]
        private float _outlinePadding = 0.18f;

        public float MinHz => _minHz;
        public float MaxHz => Mathf.Max(_minHz, _maxHz);
        public float AmberFrom => _amberFrom;
        public float DangerFrom => Mathf.Max(_amberFrom, _dangerFrom);
        public Color TagBackingColor => _tagBackingColor;
        public Vector2 TagSize => _tagSize;
        public float TagBorder => _tagBorder;
        public float OutlinePadding => _outlinePadding;

        /// <summary>The flash colour of an alarm level.</summary>
        public Color ColorFor(FlashLevel level)
        {
            if (level == FlashLevel.Danger) return _dangerColor;
            return level == FlashLevel.Amber ? _amberColor : _calmColor;
        }
    }
}
