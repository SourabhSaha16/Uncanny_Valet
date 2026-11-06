using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UncannyValet.UI
{
    /// <summary>
    /// IMGUI debug panel built from <see cref="TuningCatalog"/>: a slider, number field and reset per key, plus
    /// Reset all. F2 toggles it. It never pauses the game and writes only through <see cref="ITuning.Set"/>.
    /// Tuned values persist across runs; Reset all returns every key to its seeded value.
    /// </summary>
    public sealed class TuningPanel : MonoBehaviour
    {
        /// <summary>Prefix of every number field's control name; used to tell a typing focus from other focus.</summary>
        public const string FieldPrefix = "tuning_field_";
        /// <summary>Text for the overlay to show.</summary>
        public const string Hint = "F2: tuning panel";

        [SerializeField, Min(8)] private int _fontSize = 14;
        [SerializeField, Min(300f)] private float _width = 640f;
        [SerializeField, Range(0f, 1f)] private float _backgroundAlpha = 0.85f;
        [SerializeField, Min(0f)] private float _margin = 10f;

        private readonly List<TuningRowView> _rows = new List<TuningRowView>();
        private ITuning _tuning;
        private Vector2 _scroll;
        private bool _open;
        private bool _closeRequested;
        private GUIStyle _label;
        private GUIStyle _header;
        private GUIStyle _field;
        private GUIStyle _title;
        private GUIStyle _panelBox;
        private Texture2D _dark;

        /// <summary>True while a number field has keyboard focus, so valet keys must be ignored.</summary>
        public bool IsTyping { get; private set; }

        private void Start()
        {
            _tuning = Services.Get<ITuning>();
            BuildRows();
        }

        private void BuildRows()
        {
            string group = null;
            for (var i = 0; i < TuningCatalog.All.Count; i++)
            {
                var info = TuningCatalog.All[i];
                if (info.Group != group) { _rows.Add(TuningRowView.Header(info)); group = info.Group; }
                _rows.Add(TuningRowView.Row(info, i));
            }
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || !kb.f2Key.wasPressedThisFrame) return;
            _open = !_open;
            if (!_open) _closeRequested = true;
        }

        private void OnGUI()
        {
            if (_tuning == null) return;
            if (_closeRequested) { GUI.FocusControl(string.Empty); _closeRequested = false; IsTyping = false; }
            if (!_open) return;
            EnsureStyles();
            var panel = new Rect(Screen.width - _width - _margin, _margin, _width, Screen.height - _margin * 2f);
            GUI.Box(panel, GUIContent.none, _panelBox);
            GUI.Label(new Rect(panel.x + 8f, panel.y + 4f, 300f, 26f), "Tuning (F2 closes)", _title);
            if (GUI.Button(new Rect(panel.xMax - 108f, panel.y + 4f, 100f, 24f), "Reset all")) ResetAll();
            DrawRows(new Rect(panel.x + 6f, panel.y + 34f, panel.width - 12f, panel.height - 40f));
        }

        private void DrawRows(Rect area)
        {
            var focused = GUI.GetNameOfFocusedControl();
            IsTyping = focused.StartsWith(FieldPrefix, System.StringComparison.Ordinal);
            var content = new Rect(0f, 0f, area.width - 18f, _rows.Count * TuningRowView.RowHeight);
            _scroll = GUI.BeginScrollView(area, _scroll, content);
            for (var i = 0; i < _rows.Count; i++)
            {
                var row = new Rect(0f, i * TuningRowView.RowHeight, content.width, TuningRowView.RowHeight);
                _rows[i].Draw(row, _tuning, focused, _label, _header, _field);
            }
            GUI.EndScrollView();
        }

        private void ResetAll()
        {
            for (var i = 0; i < TuningCatalog.All.Count; i++) _tuning.Reset(TuningCatalog.All[i].Key);
        }

        private void EnsureStyles()
        {
            if (_label != null) return;
            _dark = new Texture2D(1, 1);
            _dark.SetPixel(0, 0, new Color(0f, 0f, 0f, _backgroundAlpha));
            _dark.Apply();
            _label = new GUIStyle(GUI.skin.label) { fontSize = _fontSize, normal = { textColor = Color.white, background = null } };
            _header = new GUIStyle(_label) { fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.85f, 0.3f) } };
            _title = new GUIStyle(_header) { fontSize = _fontSize + 4 };
            _field = new GUIStyle(GUI.skin.textField) { fontSize = _fontSize };
            _panelBox = new GUIStyle(GUI.skin.box) { normal = { background = _dark } };
        }

        private void OnDestroy()
        {
            if (_dark != null) Destroy(_dark);
        }
    }
}
