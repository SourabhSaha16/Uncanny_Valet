using System;
using System.Text;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.UI
{
    /// <summary>
    /// IMGUI debug overlay: clock, debt, tips, open tickets (with spot), interact prompt, pickup highlight.
    /// Core-only: ticket data comes from events via TicketBoard; prompt and clock format are delegates.
    /// </summary>
    public sealed class DebugOverlay : MonoBehaviour
    {
        [SerializeField, Min(8)] private int _fontSize = 20;
        [SerializeField, Range(0f, 1f)] private float _backgroundAlpha = 0.6f;
        [SerializeField, Min(0f)] private float _padding = 6f;
        [SerializeField, Min(1)] private int _maxTicketLines = 8;

        private readonly StringBuilder _text = new StringBuilder();
        private Func<string> _prompt = () => string.Empty;
        private Func<float, string> _formatClock = m => ((int)m).ToString();
        private TicketBoard _board;
        private IGameClock _clock;
        private IDebtView _debt;
        private GUIStyle _label;
        private GUIStyle _box;
        private Texture2D _dark;

        public void Configure(Func<string> prompt, Func<float, string> formatClock)
        {
            _prompt = prompt ?? _prompt;
            _formatClock = formatClock ?? _formatClock;
        }

        private void OnEnable()
        {
            _clock = Services.Get<IGameClock>();
            _debt = Services.Get<IDebtView>();
            _board = new TicketBoard(Services.Get<IEventBus>());
        }

        private void OnDisable() => _board?.Dispose();

        private void OnGUI()
        {
            if (_clock == null) return;
            EnsureStyles();
            _text.Clear();
            _text.AppendLine($"Night {_clock.Night}  {_formatClock(_clock.MinuteOfNight)}");
            _text.AppendLine($"Debt left: ${_debt.DebtRemaining}   Tips tonight: ${_debt.TipsTonight}");
            _text.AppendLine($"Open tickets: {_board.Count}");
            _board.AppendLines(_text, _maxTicketLines);
            _text.AppendLine(TuningPanel.Hint);
            var content = new GUIContent(_text.ToString().TrimEnd());
            var size = _label.CalcSize(content);
            var rect = new Rect(10, 10, size.x + _padding * 2f, size.y + _padding * 2f);
            GUI.Box(rect, GUIContent.none, _box);
            GUI.Label(new Rect(10 + _padding, 10 + _padding, size.x + 2f, size.y), content, _label);
            DrawPrompt();
            if (_board.PickupCount > 0) DrawPickupBanner();
        }

        private void EnsureStyles()
        {
            if (_label != null) return;
            _dark = new Texture2D(1, 1);
            _dark.SetPixel(0, 0, new Color(0f, 0f, 0f, _backgroundAlpha));
            _dark.Apply();
            _box = new GUIStyle(GUI.skin.box) { normal = { background = _dark } };
            _label = new GUIStyle(GUI.skin.label) { fontSize = _fontSize, normal = { textColor = Color.white } };
        }

        private void DrawPrompt()
        {
            var prompt = _prompt();
            if (string.IsNullOrEmpty(prompt)) return;
            var style = new GUIStyle(_box) { fontSize = _fontSize, normal = { background = _dark, textColor = Color.white } };
            GUI.Box(new Rect(Screen.width * 0.5f - 130, Screen.height - 70, 260, 40), "[E] " + prompt, style);
        }

        private void DrawPickupBanner()
        {
            var old = GUI.color;
            GUI.color = Mathf.PingPong(Time.unscaledTime * 3f, 1f) > 0.5f ? Color.yellow : Color.white;
            var style = new GUIStyle(_box) { fontSize = _fontSize, normal = { background = _dark, textColor = Color.white } };
            GUI.Box(new Rect(Screen.width * 0.5f - 250, 10, 500, 40), "PICKUP REQUESTED: bring the car to the curb", style);
            GUI.color = old;
        }
    }
}
