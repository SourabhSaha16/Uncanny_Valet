using UncannyValet.Core;
using UncannyValet.Core.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UncannyValet.UI
{
    /// <summary>
    /// Modal end-of-night card (IMGUI, no assets). Shows on NightSummaryReady, hides on NightStarted.
    /// Next / Enter / Space publishes NextNightRequested; after the final night the same button means "new run".
    /// </summary>
    public sealed class NightSummaryCard : MonoBehaviour
    {
        [SerializeField, Min(8)] private int _fontSize = 24;
        [SerializeField, Min(100f)] private float _cardWidth = 520f;
        [SerializeField, Min(0f)] private float _padding = 16f;
        [SerializeField, Min(10f)] private float _buttonHeight = 48f;
        [SerializeField, Range(0f, 1f)] private float _backdropAlpha = 0.85f;
        [SerializeField, Min(0f), Tooltip("Seconds before keys work, so a held key from play cannot skip the card.")]
        private float _keyDelaySeconds = 0.4f;

        private IEventBus _bus;
        private SummaryCardContent _content;
        private float _shownAt;
        private GUIStyle _title;
        private GUIStyle _line;
        private GUIStyle _button;
        private GUIStyle _box;
        private Texture2D _dark;

        public bool IsOpen => _content != null;

        private void OnEnable()
        {
            _bus = Services.Get<IEventBus>();
            _bus.Subscribe<NightSummaryReady>(OnSummary);
            _bus.Subscribe<NightStarted>(OnNightStarted);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<NightSummaryReady>(OnSummary);
            _bus.Unsubscribe<NightStarted>(OnNightStarted);
        }

        private void OnDestroy()
        {
            if (_dark != null) Destroy(_dark);
        }

        private void OnSummary(NightSummaryReady e)
        {
            _content = SummaryCardFormatter.Build(e.Summary);
            _shownAt = Time.unscaledTime;
        }

        private void OnNightStarted(NightStarted e) => _content = null;

        private void Update()
        {
            if (_content == null || Time.unscaledTime - _shownAt < _keyDelaySeconds) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame
                || keyboard.spaceKey.wasPressedThisFrame) Press();
        }

        private void Press()
        {
            if (_content == null) return;
            var night = _content.Night;
            _content = null;
            _bus.Publish(new NextNightRequested(night));
        }

        private void OnGUI()
        {
            if (_content == null) return;
            GUI.depth = -100;
            EnsureStyles();
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, _box);
            var lineHeight = _fontSize * 1.6f;
            var height = SummaryCardLayout.Height(_content.Lines.Length, _content.HasHeadline, lineHeight, _padding, _buttonHeight);
            SummaryCardLayout.Centre(Screen.width, Screen.height, _cardWidth, height, out var x, out var y);
            GUI.Box(new Rect(x, y, _cardWidth, height), GUIContent.none, _box);
            DrawText(x, y, lineHeight);
            var button = new Rect(x + _padding, y + height - _padding - _buttonHeight, _cardWidth - _padding * 2f, _buttonHeight);
            if (GUI.Button(button, _content.ButtonLabel, _button)) Press();
        }

        private void DrawText(float x, float y, float lineHeight)
        {
            var top = y + _padding;
            var width = _cardWidth - _padding * 2f;
            GUI.Label(new Rect(x + _padding, top, width, lineHeight), _content.Title, _title);
            top += lineHeight;
            if (_content.HasHeadline)
            {
                GUI.Label(new Rect(x + _padding, top, width, lineHeight), _content.Headline, _title);
                top += lineHeight;
            }
            foreach (var text in _content.Lines)
            {
                GUI.Label(new Rect(x + _padding, top, width, lineHeight), text, _line);
                top += lineHeight;
            }
        }

        private void EnsureStyles()
        {
            if (_title != null) return;
            _dark = new Texture2D(1, 1);
            _dark.SetPixel(0, 0, new Color(0f, 0f, 0f, _backdropAlpha));
            _dark.Apply();
            _box = new GUIStyle(GUI.skin.box) { normal = { background = _dark } };
            _title = new GUIStyle(GUI.skin.label) { fontSize = _fontSize + 6, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _title.normal.textColor = Color.white;
            _line = new GUIStyle(GUI.skin.label) { fontSize = _fontSize, alignment = TextAnchor.MiddleLeft };
            _line.normal.textColor = Color.white;
            _button = new GUIStyle(GUI.skin.button) { fontSize = _fontSize, fontStyle = FontStyle.Bold };
        }
    }
}
