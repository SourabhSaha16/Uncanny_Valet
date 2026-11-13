using UncannyValet.UI.Hud;
using UnityEngine;
using UnityEngine.Serialization;

namespace UncannyValet.UI.Mood
{
    /// <summary>
    /// Tunables of the pickup emotions: where patience turns impatient and angry, how fast the plate flashes in each
    /// mood, and the mood colours. Read only at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/HUD Emotion Config", fileName = "HudEmotionConfig")]
    public sealed class HudEmotionConfig : ScriptableObject
    {
        [Header("Patience levels (fraction of the pickup patience that has run out)")]
        [SerializeField, Range(0f, 1f), Tooltip("From here the patron is impatient (yellow). Same as the car flash's amber level.")]
        private float _impatientFrom = 0.6f;
        [SerializeField, Range(0f, 1f), Tooltip("From here the patron is angry (red). Seeds the F2 row 'Patron gets angry at'.")]
        private float _angryFrom = 0.8f;
        [SerializeField, Range(0f, 1f), Tooltip("From here, or once the timer expired, the patron will not bless (dark red). Seeds the F2 row 'Patron will not bless at'.")]
        [FormerlySerializedAs("_willNotTipFrom")] private float _willNotBlessFrom = 1f;

        [Header("Flash speed (flashes per second)")]
        [SerializeField, Min(0.1f), Tooltip("Plate flash of a happy patron.")]
        private float _happyHz = 1.5f;
        [SerializeField, Min(0.1f), Tooltip("Plate flash of an impatient patron.")]
        private float _impatientHz = 3f;
        [SerializeField, Min(0.1f), Tooltip("Plate flash of an angry patron.")]
        private float _angryHz = 7f;
        [SerializeField, Min(0.1f), Tooltip("Plate flash of a patron who will not bless: slow, they have given up.")]
        [FormerlySerializedAs("_willNotTipHz")] private float _willNotBlessHz = 1f;

        [Header("Colours")]
        [SerializeField] private Color _happyColor = new Color32(0x4C, 0xCB, 0x5E, 255);
        [SerializeField] private Color _impatientColor = new Color32(0xFF, 0xE0, 0x3A, 255);
        [SerializeField] private Color _angryColor = new Color32(0xF0, 0x3A, 0x2A, 255);
        [SerializeField] [FormerlySerializedAs("_willNotTipColor")] private Color _willNotBlessColor = new Color32(0xA0, 0x28, 0x28, 255);

        [Header("Blessing pop over a boarding patron")]
        [SerializeField] [FormerlySerializedAs("_tipPop")] private BlessingPopSettings _blessingPop = new BlessingPopSettings();

        [Header("Power bar (right edge)")]
        [FormerlySerializedAs("_powerMeter")]
        [SerializeField] private PowerBarSettings _powerBar = new PowerBarSettings();

        public BlessingPopSettings BlessingPop => _blessingPop;
        public PowerBarSettings PowerBar => _powerBar;
        public float ImpatientFrom => _impatientFrom;
        public float AngryFrom => _angryFrom;
        public float WillNotBlessFrom => _willNotBlessFrom;
        public float WillNotBlessHz => _willNotBlessHz;
        public Color WillNotBlessColor => _willNotBlessColor;
        public float HappyHz => _happyHz;
        public float ImpatientHz => _impatientHz;
        public float AngryHz => _angryHz;
        public Color HappyColor => _happyColor;
        public Color ImpatientColor => _impatientColor;
        public Color AngryColor => _angryColor;
    }
}
