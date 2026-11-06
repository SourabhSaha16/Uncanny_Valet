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
        [SerializeField, Min(8)] private int _fontSize = 18;
        [SerializeField, Min(100f)] private float _cardWidth = 520f;
        [SerializeField, Min(0f)] private float _padding = 14f;
        [SerializeField, Min(10f)] private float _buttonHeight = 44f;
        [SerializeField, Min(1f), Tooltip("Row height as a multiple of the font size.")] private float _lineSpacing = 1.5f;
        [SerializeField, Min(0.1f), Tooltip("Average glyph width as a multiple of the font size, for estimating note wrapping.")]
        private float _glyphWidth = 0.6f;
        [SerializeField, Min(0f), Tooltip("Space kept above and below the card; the card shrinks to fit the screen.")]
        private float _screenMargin = 10f;
        [SerializeField, Range(0f, 1f)] private float _backdropAlpha = 0.85f;
        [SerializeField] private Color _tomorrowColor = new Color(0.55f, 0.35f, 0.05f, 1f);
        [SerializeField, Min(0f), Tooltip("Seconds before keys work, so a held key from play cannot skip the card.")]
        private float _keyDelaySeconds = 0.4f;

        private IEventBus _bus;
        private SummaryCardContent _content;
        private float _shownAt;
        private SummaryCardStyles _styles;

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
            _styles?.Dispose();
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
            _styles ??= new SummaryCardStyles(_fontSize, _backdropAlpha, _tomorrowColor);
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, _styles.Box);
            var lineHeight = _fontSize * _lineSpacing;
            var noteRows = SummaryCardLayout.WrappedLineCount(_content.Tomorrow, NoteChars());
            var height = SummaryCardLayout.Height(_content.Lines.Length, _content.HasHeadline,
                _content.CalcLines.Length, noteRows, lineHeight, _padding, _buttonHeight);
            SummaryCardLayout.Centre(Screen.width, Screen.height, _cardWidth, height, out var x, out var y);
            var old = GUI.matrix;
            var centre = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            var scale = SummaryCardLayout.FitScale(height, Screen.height, _screenMargin);
            GUI.matrix = Matrix4x4.TRS(centre, Quaternion.identity, new Vector3(scale, scale, 1f))
                * Matrix4x4.TRS(-centre, Quaternion.identity, Vector3.one);
            GUI.Box(new Rect(x, y, _cardWidth, height), GUIContent.none, _styles.Box);
            DrawText(x, y, lineHeight, noteRows);
            var button = new Rect(x + _padding, y + height - _padding - _buttonHeight, _cardWidth - _padding * 2f, _buttonHeight);
            if (GUI.Button(button, _content.ButtonLabel, _styles.Button)) Press();
            GUI.matrix = old;
        }

        private int NoteChars() => Mathf.Max(1, (int)((_cardWidth - _padding * 2f - 16f) / ((_fontSize - 2) * _glyphWidth)));

        private void DrawText(float x, float y, float lineHeight, int noteRows)
        {
            var left = x + _padding;
            var width = _cardWidth - _padding * 2f;
            var top = y + _padding;
            top = Row(left, top, width, lineHeight, _content.Title, _styles.Title);
            if (_content.HasHeadline) top = Row(left, top, width, lineHeight, _content.Headline, _styles.Title);
            foreach (var text in _content.Lines) top = Row(left, top, width, lineHeight, text, _styles.Line);
            if (_content.HasCalc)
            {
                top = Row(left, top, width, lineHeight, SummaryCardFormatter.CalcHeader, _styles.Header);
                foreach (var text in _content.CalcLines) top = Row(left + _padding, top, width - _padding, lineHeight, text, _styles.Line);
            }
            if (noteRows > 0)
            {
                top += _padding;
                GUI.Label(new Rect(left, top, width, noteRows * lineHeight + 8f), _content.Tomorrow, _styles.Note);
            }
        }

        private static float Row(float x, float top, float width, float height, string text, GUIStyle style)
        {
            GUI.Label(new Rect(x, top, width, height), text, style);
            return top + height;
        }
    }
}
