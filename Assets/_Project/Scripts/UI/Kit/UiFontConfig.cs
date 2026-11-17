using UnityEngine;

namespace UncannyValet.UI.Kit
{
    /// <summary>
    /// The UI font and how big it is drawn. The font is a monospaced TrueType face (JetBrains Mono Bold, SIL OFL 1.1) so every glyph
    /// still sits in the same 6x7 virtual-pixel cell the layouts are built on; only the drawing is smoother. Loaded from Resources.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/UI Font Config", fileName = "UiFontConfig")]
    public sealed class UiFontConfig : ScriptableObject
    {
        /// <summary>Resources path (no extension) of the shipped asset.</summary>
        public const string ResourcePath = "UiFontConfig";

        [SerializeField, Tooltip("The TrueType font every UI letter is drawn with. Must be monospaced so letters keep the cell grid.")]
        private Font _font;
        [SerializeField, Range(0.8f, 2f), Tooltip("Font size (em) as a multiple of one text row's height. About 1.37 makes capitals exactly one row tall and letters one cell wide.")]
        private float _emPerRow = 1.37f;
        [SerializeField, Min(8), Tooltip("Largest font size in screen pixels; bigger text is drawn at this size and stretched, so the glyph atlas stays small.")]
        private int _maxFontSize = 96;
        [SerializeField, Min(1), Tooltip("While the canvas is zoomed out (pops shrinking), sizes snap to a multiple of this so the atlas does not fill with sizes.")]
        private int _zoomSizeStep = 8;
        [SerializeField, Range(-0.2f, 0.2f), Tooltip("Moves letters down (positive) or up inside their row, as a fraction of the font size.")]
        private float _verticalNudge;

        public Font Font => _font;
        public float EmPerRow => _emPerRow;
        public int MaxFontSize => _maxFontSize;
        public int ZoomSizeStep => _zoomSizeStep;
        public float VerticalNudge => _verticalNudge;

        /// <summary>Loads the shipped config; logs an error and falls back to defaults (the built-in font) when it is missing.</summary>
        public static UiFontConfig LoadOrDefault()
        {
            var config = Resources.Load<UiFontConfig>(ResourcePath);
            if (config != null) return config;
            Debug.LogError("UiFontConfig is missing from Resources; the UI falls back to the built-in font.");
            return CreateInstance<UiFontConfig>();
        }
    }
}
