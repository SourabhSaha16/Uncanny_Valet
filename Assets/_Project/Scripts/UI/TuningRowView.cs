using System.Globalization;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.UI
{
    /// <summary>
    /// Draws one tuning row (label, slider, number field, reset) or a group header. Strings are cached and only
    /// rebuilt when the value changes, so steady-state drawing allocates nothing of its own.
    /// </summary>
    public sealed class TuningRowView
    {
        public const float RowHeight = 26f;
        private const float LabelWidth = 400f;
        private const float FieldWidth = 64f;
        private const float ResetWidth = 52f;
        private const float Gap = 6f;

        private readonly TuningInfo _info;
        private readonly bool _isHeader;
        private readonly string _fieldName;
        private readonly GUIContent _content;
        private readonly string _format;
        private string _text = string.Empty;
        private float _shown = float.NaN;
        private bool _dirty;

        private TuningRowView(TuningInfo info, bool isHeader, string fieldName)
        {
            _info = info;
            _isHeader = isHeader;
            _fieldName = fieldName;
            _format = info.Step < 1f ? "0.##" : "0";
            _content = new GUIContent(info.Label, info.Tooltip);
        }

        /// <summary>True when the row only shows with the panel's Advanced toggle on.</summary>
        public bool IsAdvanced => _info.Advanced;

        /// <summary>The control name of this row's number field; empty for headers.</summary>
        public string FieldName => _fieldName;

        public static TuningRowView Header(TuningInfo first) => new TuningRowView(first, true, string.Empty);

        public static TuningRowView Row(TuningInfo info, int index) =>
            new TuningRowView(info, false, TuningPanel.FieldPrefix + index);

        /// <summary>Draws the row and writes any change through <paramref name="tuning"/>.</summary>
        public void Draw(Rect rect, ITuning tuning, string focusedField, GUIStyle label, GUIStyle header, GUIStyle field)
        {
            if (_isHeader) { GUI.Label(rect, _info.Group, header); return; }
            if (!tuning.TryGet(_info.Key, out var value)) return;

            GUI.Label(new Rect(rect.x, rect.y, LabelWidth, rect.height), _content, label);
            var sliderX = rect.x + LabelWidth + Gap;
            var fieldX = rect.xMax - ResetWidth - Gap - FieldWidth;
            var slider = new Rect(sliderX, rect.y + 6f, fieldX - sliderX - Gap, rect.height - 6f);
            var moved = GUI.HorizontalSlider(slider, value, _info.Min, _info.Max);
            if (!Mathf.Approximately(moved, value) && Write(tuning, moved, ref value)) Refresh(value);

            DrawField(new Rect(fieldX, rect.y, FieldWidth, rect.height - 2f), tuning, focusedField, field, ref value);
            var reset = new Rect(rect.xMax - ResetWidth, rect.y, ResetWidth, rect.height - 2f);
            if (GUI.Button(reset, "reset")) tuning.Reset(_info.Key);
        }

        private void DrawField(Rect rect, ITuning tuning, string focusedField, GUIStyle style, ref float value)
        {
            var focused = focusedField == _fieldName;
            if (!focused && (_dirty || !Mathf.Approximately(_shown, value))) Refresh(value);
            GUI.SetNextControlName(_fieldName);
            var typed = GUI.TextField(rect, _text, 8, style);
            if (typed == _text) return;
            _text = typed;
            _dirty = true;
            if (float.TryParse(typed, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                Write(tuning, parsed, ref value);
        }

        private bool Write(ITuning tuning, float raw, ref float value)
        {
            var snapped = Mathf.Round(raw / _info.Step) * _info.Step;
            var clamped = Mathf.Clamp(snapped, _info.Min, _info.Max);
            if (Mathf.Approximately(clamped, value)) return false;
            tuning.Set(_info.Key, clamped);
            value = clamped;
            return true;
        }

        private void Refresh(float value)
        {
            _shown = value;
            _dirty = false;
            _text = value.ToString(_format, CultureInfo.InvariantCulture);
        }
    }
}
