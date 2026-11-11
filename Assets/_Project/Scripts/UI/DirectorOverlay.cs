using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UncannyValet.UI
{
    /// <summary>
    /// IMGUI debug overlay for the Patron Director. F3 toggles it; it steps aside while the F2 tuning panel is open.
    /// Shows each station slot in its state colour, the car counts, free spots, W / T / P / F, the phase, the arrival
    /// counters and the last five decisions. Text is rebuilt a few times a second, not every frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DirectorOverlay : MonoBehaviour
    {
        private const int LogCapacity = 5;

        [SerializeField, Tooltip("The F2 panel; the overlay hides while it is open. Optional.")]
        private TuningPanel _tuningPanel;
        [SerializeField, Min(8)] private int _fontSize = 13;
        [SerializeField, Min(200f)] private float _width = 430f;
        [SerializeField, Min(0.05f)] private float _refreshSeconds = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _backgroundAlpha = 0.8f;
        [SerializeField, Min(0f)] private float _margin = 10f;
        [SerializeField, Min(8f)] private float _swatchSize = 14f;

        private readonly List<StationSlot> _slots = new List<StationSlot>(8);
        private readonly List<string> _slotTexts = new List<string>(8);
        private readonly List<string> _decisions = new List<string>(LogCapacity);
        private IDirectorView _view;
        private ILotSnapshotSource _source;
        private bool _open;
        private float _nextRefresh;
        private string _header = string.Empty;
        private string _arrivals = string.Empty;
        private string _cars = string.Empty;
        private string _dilemma = string.Empty;
        private string _dilemmaCounters = string.Empty;
        private GUIStyle _label;
        private GUIStyle _title;
        private GUIStyle _box;
        private Texture2D _dark;

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.f3Key.wasPressedThisFrame) _open = !_open;
            if (!_open || Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + _refreshSeconds;
            Refresh();
        }

        // Services are registered by Bootstrap before this object is activated, but resolve lazily to stay order-proof.
        private void Refresh()
        {
            if (_view == null) Services.TryGet(out _view);
            if (_source == null) Services.TryGet(out _source);
            if (_view == null || _source == null) return;
            var snapshot = _source.Capture();
            _header = DirectorOverlayText.Header(_view);
            _arrivals = DirectorOverlayText.Arrivals(_view);
            _cars = DirectorOverlayText.Cars(snapshot);
            _dilemma = DirectorOverlayText.Dilemma(_view);
            _dilemmaCounters = DirectorOverlayText.DilemmaCounters(_view);
            CopySlots(snapshot);
            CopyDecisions();
        }

        private void CopySlots(in LotSnapshot snapshot)
        {
            _slots.Clear();
            _slotTexts.Clear();
            var bay = 0;
            for (var i = 0; i < snapshot.Slots.Count; i++)
            {
                var slot = snapshot.Slots[i];
                if (slot.Kind == StationKind.Charging) bay++;
                _slots.Add(slot);
                _slotTexts.Add(DirectorOverlayText.Slot(slot, bay));
            }
        }

        private void CopyDecisions()
        {
            _decisions.Clear();
            for (var i = 0; i < _view.DecisionCount; i++) _decisions.Add(DirectorOverlayText.Decision(_view.GetDecision(i)));
        }

        private void OnGUI()
        {
            if (!_open || _view == null || string.IsNullOrEmpty(_header)) return;
            if (_tuningPanel != null && _tuningPanel.IsOpen) return;
            EnsureStyles();
            var line = _fontSize + 6f;
            var rows = 8 + _slots.Count + _decisions.Count;
            var panel = new Rect(_margin, _margin, _width, line * rows + 16f);
            GUI.Box(panel, GUIContent.none, _box);
            var y = panel.y + 6f;
            GUI.Label(new Rect(panel.x + 8f, y, _width - 16f, line), "Director (F3 closes)", _title);
            y += line;
            y = Line(panel, y, line * 2f, _header);
            y = Line(panel, y, line, _arrivals);
            y = Line(panel, y, line, _dilemma);
            y = Line(panel, y, line, _dilemmaCounters);
            y = DrawSlots(panel, y, line);
            y = Line(panel, y, line * 2f, _cars);
            for (var i = 0; i < _decisions.Count; i++) y = Line(panel, y, line, _decisions[i]);
        }

        private float DrawSlots(Rect panel, float y, float line)
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                GUI.color = DirectorOverlayText.SlotColor(_slots[i].State);
                GUI.DrawTexture(new Rect(panel.x + 8f, y + 2f, _swatchSize, _swatchSize), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(panel.x + 12f + _swatchSize, y, _width - 24f - _swatchSize, line), _slotTexts[i], _label);
                y += line;
            }
            return y;
        }

        private float Line(Rect panel, float y, float height, string text)
        {
            GUI.Label(new Rect(panel.x + 8f, y, _width - 16f, height), text, _label);
            return y + height;
        }

        private void EnsureStyles()
        {
            if (_label != null) return;
            _dark = new Texture2D(1, 1);
            _dark.SetPixel(0, 0, new Color(0f, 0f, 0f, _backgroundAlpha));
            _dark.Apply();
            _label = new GUIStyle(GUI.skin.label) { fontSize = _fontSize, normal = { textColor = Color.white } };
            _title = new GUIStyle(_label) { fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.85f, 0.3f) } };
            _box = new GUIStyle(GUI.skin.box) { normal = { background = _dark } };
        }

        private void OnDestroy()
        {
            if (_dark != null) Destroy(_dark);
        }
    }
}
