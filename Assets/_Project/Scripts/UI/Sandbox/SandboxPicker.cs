using UncannyValet.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UncannyValet.UI.Sandbox
{
    /// <summary>
    /// The sandbox's world picker: a column of buttons (keys 1 to 9 do the same) and the R key that resets the car. IMGUI, no UI assets.
    /// Keys are ignored while a number field of the F2 panel has focus.
    /// </summary>
    public sealed class SandboxPicker : MonoBehaviour
    {
        /// <summary>Most worlds the number keys reach.</summary>
        public const int MaxKeyedWorlds = 9;
        private const string Hint = "R: reset car   E: get in or out   F2: tuning panel";
        private const float Margin = 10f;
        private const float ButtonHeight = 30f;
        private const float ButtonGap = 4f;
        private const float ButtonWidth = 300f;
        private const float HintWidth = 600f;

        [SerializeField, Tooltip("The F2 panel; its typing focus blocks the keys here.")]
        private TuningPanel _tuningPanel;
        [SerializeField, Min(8)] private int _fontSize = 14;

        private ISandboxControl _control;
        private string[] _labels;
        private GUIStyle _button;
        private GUIStyle _current;
        private GUIStyle _hint;

        private void Start()
        {
            if (!Services.TryGet(out _control)) { Debug.LogError("SandboxPicker found no ISandboxControl.", this); enabled = false; return; }
            _labels = new string[_control.WorldCount];
            for (var i = 0; i < _labels.Length; i++)
                _labels[i] = (i < MaxKeyedWorlds ? (i + 1).ToString() : "-") + "  " + _control.WorldName(i);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || (_tuningPanel != null && _tuningPanel.IsTyping)) return;
            if (kb.rKey.wasPressedThisFrame) _control.ResetCar();
            var keyed = Mathf.Min(_labels.Length, MaxKeyedWorlds);
            for (var i = 0; i < keyed; i++)
                if (kb[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame) _control.TrySelectWorld(i);
        }

        private void OnGUI()
        {
            if (_control == null) return;
            EnsureStyles();
            for (var i = 0; i < _labels.Length; i++)
            {
                var rect = new Rect(Margin, Margin + i * (ButtonHeight + ButtonGap), ButtonWidth, ButtonHeight);
                if (GUI.Button(rect, _labels[i], i == _control.CurrentWorld ? _current : _button)) _control.TrySelectWorld(i);
            }
            GUI.Label(new Rect(Margin, Margin + _labels.Length * (ButtonHeight + ButtonGap), HintWidth, ButtonHeight), Hint, _hint);
        }

        private void EnsureStyles()
        {
            if (_button != null) return;
            _button = new GUIStyle(GUI.skin.button) { fontSize = _fontSize, alignment = TextAnchor.MiddleLeft };
            _current = new GUIStyle(_button) { fontStyle = FontStyle.Bold };
            _current.normal.textColor = Color.yellow;
            _current.hover.textColor = Color.yellow;
            _hint = new GUIStyle(GUI.skin.label) { fontSize = _fontSize };
            _hint.normal.textColor = Color.white;
        }
    }
}
