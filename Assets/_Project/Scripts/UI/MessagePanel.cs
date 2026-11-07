using System.Text;
using UncannyValet.Core;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.UI
{
    /// <summary>
    /// IMGUI boss panel on the right: header, strike marks and the newest boss messages (oldest at the top,
    /// newest at the bottom). It starts below the pickup banner and hides while the tuning panel is open.
    /// Silent: sound belongs to the audio module.
    /// </summary>
    public sealed class MessagePanel : MonoBehaviour
    {
        [SerializeField, Min(8)] private int _fontSize = 16;
        [SerializeField, Min(150f)] private float _width = 300f;
        [SerializeField, Range(0.1f, 0.6f), Tooltip("The panel never takes more than this share of the screen width.")]
        private float _maxScreenShare = 0.3f;
        [SerializeField, Min(0f)] private float _margin = 10f;
        [SerializeField, Min(0f), Tooltip("Space kept clear at the top for the pickup banner (top centre).")]
        private float _topReserve = 130f;
        [SerializeField, Min(20f)] private float _cardHeight = 52f;
        [SerializeField, Min(0f)] private float _padding = 6f;
        [SerializeField, Range(0f, 1f)] private float _backgroundAlpha = 0.7f;
        [SerializeField, Range(1, 8)] private int _maxVisible = 5;
        [SerializeField, Min(1)] private int _defaultMaxStrikes = 3;
        [SerializeField, Min(0f)] private float _doneSeconds = 4f;
        [SerializeField, Min(0f)] private float _failedSeconds = 8f;
        [SerializeField, Min(0.1f)] private float _fadeSeconds = 1f;
        [SerializeField, Min(0.1f)] private float _flashSeconds = 1.2f;
        [SerializeField, Tooltip("Optional. The panel hides while this debug panel is open, since both sit on the right.")]
        private TuningPanel _tuningPanel;

        private readonly StringBuilder _strikeText = new StringBuilder();
        private MessageFeed _feed;
        private MessagePanelStyles _styles;
        private string _strikeString = string.Empty;
        private int _shownStrikes = -1;
        private int _shownMax = -1;

        private void OnEnable()
        {
            var settings = new MessageFeedSettings(_maxVisible, _defaultMaxStrikes, _doneSeconds,
                _failedSeconds, _fadeSeconds, _flashSeconds);
            _feed = new MessageFeed(Services.Get<IEventBus>(), () => Time.unscaledTime, settings);
        }

        private void OnDisable() => _feed?.Dispose();

        private void OnDestroy() => _styles?.Dispose();

        private void Update() => _feed?.Tick();

        private void OnGUI()
        {
            if (_feed == null || (_tuningPanel != null && _tuningPanel.IsOpen)) return;
            _styles ??= new MessagePanelStyles(_fontSize, _backgroundAlpha);
            RefreshStrikes();
            var width = Mathf.Min(_width, Screen.width * _maxScreenShare);
            var header = _fontSize * 2f + _padding;
            var height = header + _padding + _feed.Count * (_cardHeight + _padding);
            var top = _margin + _topReserve;
            var scale = Mathf.Min(1f, Mathf.Max(0.1f, (Screen.height - top - _margin) / height));
            var left = Screen.width - _margin - width * scale;
            var old = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(left, top, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
            GUI.Box(new Rect(0f, 0f, width, height), GUIContent.none, _styles.Box);
            GUI.Label(new Rect(_padding, _padding, width * 0.4f, header - _padding), MessageFormatter.Header, _styles.Header);
            GUI.Label(new Rect(width * 0.4f, _padding, width * 0.6f - _padding, header - _padding), _strikeString, _styles.Strikes);
            DrawCards(width, header + _padding);
            GUI.matrix = old;
        }

        private void DrawCards(float width, float top)
        {
            for (var i = 0; i < _feed.Count; i++)
            {
                var m = _feed[i];
                var rect = new Rect(_padding, top + i * (_cardHeight + _padding), width - _padding * 2f, _cardHeight);
                var old = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, _feed.Alpha(m));
                GUI.Box(rect, GUIContent.none, _styles.Box);
                DrawFlash(rect, m);
                GUI.Label(new Rect(rect.x + _padding, rect.y, rect.width - _padding * 2f, rect.height * 0.55f), m.Text, _styles.Text);
                GUI.Label(new Rect(rect.x + _padding, rect.y + rect.height * 0.5f, rect.width - _padding * 2f, rect.height * 0.45f),
                    MessageFormatter.Badge(m.Status), _styles.Badge(m.Status));
                GUI.color = old;
            }
        }

        private void DrawFlash(Rect rect, FeedMessage m)
        {
            var flash = _feed.Flash(m);
            if (flash <= 0f) return;
            var before = GUI.color;
            GUI.color = new Color(1f, 0.9f, 0.3f, flash * 0.6f * before.a);
            GUI.DrawTexture(rect, _styles.White);
            GUI.color = before;
        }

        private void RefreshStrikes()
        {
            if (_feed.Strikes == _shownStrikes && _feed.MaxStrikes == _shownMax) return;
            _shownStrikes = _feed.Strikes;
            _shownMax = _feed.MaxStrikes;
            _strikeText.Clear();
            MessageFormatter.AppendStrikes(_strikeText, _shownStrikes, _shownMax);
            _strikeString = _strikeText.ToString();
        }
    }
}
