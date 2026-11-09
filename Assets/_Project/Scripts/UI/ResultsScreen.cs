using UncannyValet.Core;
using UncannyValet.Core.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UncannyValet.UI
{
    /// <summary>
    /// Modal end-of-shift screen (IMGUI, no assets): stars, score and one Retry button. Shows on
    /// ShiftResultReady and hides on ShiftStarted. Retry / Enter / Space publishes RetryShiftRequested.
    /// </summary>
    public sealed class ResultsScreen : MonoBehaviour
    {
        [SerializeField, Min(8)] private int _fontSize = 20;
        [SerializeField, Min(1), Tooltip("The star row is this many times bigger than the body text.")] private int _starScale = 3;
        [SerializeField, Min(100f)] private float _cardWidth = 420f;
        [SerializeField, Min(0f)] private float _padding = 16f;
        [SerializeField, Min(10f)] private float _buttonHeight = 48f;
        [SerializeField, Min(1f), Tooltip("Row height as a multiple of the font size.")] private float _lineSpacing = 1.6f;
        [SerializeField, Range(0f, 1f)] private float _backdropAlpha = 0.85f;
        [SerializeField, Min(0f), Tooltip("Seconds before keys work, so a held key from play cannot skip the screen.")]
        private float _keyDelaySeconds = 0.4f;

        private IEventBus _bus;
        private ResultsContent _content;
        private float _shownAt;
        private ResultsStyles _styles;

        public bool IsOpen => _content != null;

        private void OnEnable()
        {
            _bus = Services.Get<IEventBus>();
            _bus.Subscribe<ShiftResultReady>(OnResult);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<ShiftResultReady>(OnResult);
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
        }

        private void OnDestroy() => _styles?.Dispose();

        private void OnResult(ShiftResultReady e)
        {
            _content = ResultsFormatter.Build(e.Result);
            _shownAt = Time.unscaledTime;
        }

        private void OnShiftStarted(ShiftStarted e) => _content = null;

        private void Update()
        {
            if (_content == null || Time.unscaledTime - _shownAt < _keyDelaySeconds) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame
                || keyboard.spaceKey.wasPressedThisFrame) Retry();
        }

        private void Retry()
        {
            if (_content == null) return;
            _content = null;
            _bus.Publish(new RetryShiftRequested());
        }

        private void OnGUI()
        {
            if (_content == null) return;
            GUI.depth = -100;
            _styles ??= new ResultsStyles(_fontSize, _backdropAlpha, _starScale);
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, _styles.Box);
            var lineHeight = _fontSize * _lineSpacing;
            var starHeight = _fontSize * _starScale * _lineSpacing;
            var height = _padding * 3f + lineHeight * (1 + _content.Lines.Length) + starHeight + _buttonHeight;
            var x = (Screen.width - _cardWidth) * 0.5f;
            var y = (Screen.height - height) * 0.5f;
            GUI.Box(new Rect(x, y, _cardWidth, height), GUIContent.none, _styles.Box);
            DrawText(x + _padding, y + _padding, _cardWidth - _padding * 2f, lineHeight, starHeight);
            var button = new Rect(x + _padding, y + height - _padding - _buttonHeight, _cardWidth - _padding * 2f, _buttonHeight);
            if (GUI.Button(button, _content.ButtonLabel, _styles.Button)) Retry();
        }

        private void DrawText(float left, float top, float width, float lineHeight, float starHeight)
        {
            GUI.Label(new Rect(left, top, width, lineHeight), _content.Title, _styles.Title);
            top += lineHeight;
            GUI.Label(new Rect(left, top, width, starHeight), _content.Stars, _styles.Stars);
            top += starHeight;
            foreach (var text in _content.Lines)
            {
                GUI.Label(new Rect(left, top, width, lineHeight), text, _styles.Line);
                top += lineHeight;
            }
        }
    }
}
