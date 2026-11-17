using UnityEngine;

namespace UncannyValet.UI.Kit
{
    /// <summary>
    /// Draws single letters with the UI TrueType font, centred in their text cell. Letters are the cached one-character strings of
    /// <see cref="PixelGlyphs.Chars"/>, so drawing allocates nothing; the dynamic font builds each size's glyphs the first time it is used.
    /// </summary>
    public sealed class PixelFontRenderer
    {
        private readonly UiFontConfig _config;
        private readonly string[] _letters = new string[PixelGlyphs.Count];
        private readonly GUIContent _content = new GUIContent();
        private GUIStyle _style;

        public PixelFontRenderer(UiFontConfig config)
        {
            _config = config;
            for (var i = 0; i < _letters.Length; i++) _letters[i] = PixelGlyphs.Chars[i].ToString();
        }

        /// <summary>Draws one letter inside <paramref name="cell"/> (screen pixels) in the current <see cref="GUI.color"/>.</summary>
        public void Draw(char c, Rect cell, int textScale, float unit, bool zoomed)
        {
            var style = Style();
            style.fontSize = UiFontSizing.Size(textScale, unit, _config.EmPerRow, _config.MaxFontSize, zoomed ? _config.ZoomSizeStep : 1);
            cell.y += _config.VerticalNudge * style.fontSize;
            _content.text = _letters[PixelGlyphs.IndexOf(c)];
            GUI.Label(cell, _content, style);
        }

        private GUIStyle Style()
        {
            if (_style != null) return _style;
            _style = new GUIStyle
            {
                font = _config.Font,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Overflow,
                wordWrap = false,
                richText = false,
            };
            _style.normal.textColor = Color.white;
            return _style;
        }
    }
}
