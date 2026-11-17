using UncannyValet.UI.Hud;
using UncannyValet.UI.Pie;
using UncannyValet.UI.Speech;
using UnityEngine;
using UnityEngine.Serialization;

namespace UncannyValet.UI.Mood
{
    /// <summary>
    /// Tunables of the pickup emotions: where patience turns impatient and angry, the mood colours and the pickup pie. Read only at runtime.
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

        [Header("Colours")]
        [SerializeField] private Color _happyColor = new Color32(0x4C, 0xCB, 0x5E, 255);
        [SerializeField] private Color _impatientColor = new Color32(0xFF, 0xE0, 0x3A, 255);
        [SerializeField] private Color _angryColor = new Color32(0xF0, 0x3A, 0x2A, 255);
        [SerializeField] [FormerlySerializedAs("_willNotTipColor")] private Color _willNotBlessColor = new Color32(0xA0, 0x28, 0x28, 255);

        [Header("Pickup timer pie over a waiting car")]
        [SerializeField] private PickupPieSettings _pickupPie = new PickupPieSettings();

        [Header("Blessing pop over a boarding patron")]
        [SerializeField] [FormerlySerializedAs("_tipPop")] private BlessingPopSettings _blessingPop = new BlessingPopSettings();

        [Header("Power bar (right edge)")]
        [FormerlySerializedAs("_powerMeter")]
        [SerializeField] private PowerBarSettings _powerBar = new PowerBarSettings();

        [Header("Watcher speech bubble")]
        [SerializeField] private SpeechBubbleSettings _speechBubble = new SpeechBubbleSettings();

        public SpeechBubbleSettings SpeechBubble => _speechBubble;
        public PickupPieSettings PickupPie => _pickupPie;
        public BlessingPopSettings BlessingPop => _blessingPop;
        public PowerBarSettings PowerBar => _powerBar;
        public float ImpatientFrom => _impatientFrom;
        public float AngryFrom => _angryFrom;
        public float WillNotBlessFrom => _willNotBlessFrom;
        public Color WillNotBlessColor => _willNotBlessColor;
        public Color HappyColor => _happyColor;
        public Color ImpatientColor => _impatientColor;
        public Color AngryColor => _angryColor;
    }
}
