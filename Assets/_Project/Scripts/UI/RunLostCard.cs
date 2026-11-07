using System;
using System.Text;
using UncannyValet.Core;
using UncannyValet.Core.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UncannyValet.UI
{
    /// <summary>
    /// Full-screen card shown when the boss gives the final strike. While it is open the game is frozen
    /// (Time.timeScale 0, restored on close) and the button, Enter or Space starts a new run through the handler
    /// Bootstrap supplies with <see cref="Configure"/>. Shown again only after the next run starts.
    /// </summary>
    public sealed class RunLostCard : MonoBehaviour
    {
        [SerializeField, Min(8)] private int _fontSize = 20;
        [SerializeField, Min(100f)] private float _cardWidth = 580f;
        [SerializeField, Min(0f)] private float _padding = 16f;
        [SerializeField, Min(10f)] private float _buttonHeight = 48f;
        [SerializeField, Range(0f, 1f)] private float _backdropAlpha = 0.9f;
        [SerializeField, Min(0f), Tooltip("Seconds before keys work, so a held key from play cannot skip the card.")]
        private float _keyDelaySeconds = 0.6f;

        private readonly StringBuilder _detail = new StringBuilder();
        private IEventBus _bus;
        private Action _newRun;
        private SummaryCardStyles _styles;
        private string _detailText = string.Empty;
        private float _shownAt;
        private float _savedTimeScale = 1f;

        public bool IsOpen { get; private set; }

        /// <summary>Sets what the New run button does.</summary>
        public void Configure(Action newRun) => _newRun = newRun;

        private void OnEnable()
        {
            _bus = Services.Get<IEventBus>();
            _bus.Subscribe<StrikeGiven>(OnStrike);
            _bus.Subscribe<NightStarted>(OnNightStarted);
        }

        private void OnDisable()
        {
            Close();
            if (_bus == null) return;
            _bus.Unsubscribe<StrikeGiven>(OnStrike);
            _bus.Unsubscribe<NightStarted>(OnNightStarted);
        }

        private void OnDestroy() => _styles?.Dispose();

        private void OnStrike(StrikeGiven e)
        {
            if (!e.IsFinal || IsOpen) return;
            _detail.Clear();
            MessageFormatter.AppendLostDetail(_detail, e.Total, e.Max);
            _detailText = _detail.ToString();
            _shownAt = Time.unscaledTime;
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            IsOpen = true;
        }

        private void OnNightStarted(NightStarted e) => Close();

        private void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            Time.timeScale = _savedTimeScale;
        }

        private void Update()
        {
            if (!IsOpen || Time.unscaledTime - _shownAt < _keyDelaySeconds) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame
                || keyboard.spaceKey.wasPressedThisFrame) Press();
        }

        private void Press()
        {
            if (_newRun == null)
            {
                Debug.LogError("RunLostCard has no new-run handler; Bootstrap must call Configure.", this);
                return;
            }
            _newRun();
        }

        private void OnGUI()
        {
            if (!IsOpen) return;
            GUI.depth = -110;
            _styles ??= new SummaryCardStyles(_fontSize, _backdropAlpha, Color.black);
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none, _styles.Box);
            var rowHeight = _fontSize * 2f;
            var height = _padding * 3f + rowHeight * 2f + _buttonHeight;
            SummaryCardLayout.Centre(Screen.width, Screen.height, _cardWidth, height, out var x, out var y);
            var width = _cardWidth - _padding * 2f;
            GUI.Box(new Rect(x, y, _cardWidth, height), GUIContent.none, _styles.Box);
            GUI.Label(new Rect(x + _padding, y + _padding, width, rowHeight), MessageFormatter.LostTitle, _styles.Title);
            GUI.Label(new Rect(x + _padding, y + _padding + rowHeight, width, rowHeight), _detailText, _styles.Line);
            var button = new Rect(x + _padding, y + height - _padding - _buttonHeight, width, _buttonHeight);
            if (GUI.Button(button, MessageFormatter.LostButton, _styles.Button)) Press();
        }
    }
}
