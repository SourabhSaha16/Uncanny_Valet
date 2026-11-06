using UnityEngine;

namespace UncannyValet.UI
{
    /// <summary>The IMGUI styles and textures for the summary card. Created once, released with Dispose.</summary>
    public sealed class SummaryCardStyles
    {
        public readonly GUIStyle Box;
        public readonly GUIStyle Title;
        public readonly GUIStyle Line;
        public readonly GUIStyle Header;
        public readonly GUIStyle Note;
        public readonly GUIStyle Button;
        private readonly Texture2D _dark;
        private readonly Texture2D _highlight;

        public SummaryCardStyles(int fontSize, float backdropAlpha, Color highlight)
        {
            _dark = Solid(new Color(0f, 0f, 0f, backdropAlpha));
            _highlight = Solid(highlight);
            Box = new GUIStyle(GUI.skin.box) { normal = { background = _dark } };
            Title = Make(fontSize + 6, FontStyle.Bold, TextAnchor.MiddleCenter);
            Line = Make(fontSize, FontStyle.Normal, TextAnchor.MiddleLeft);
            Header = Make(fontSize, FontStyle.Bold, TextAnchor.MiddleLeft);
            Note = Make(fontSize - 2, FontStyle.Bold, TextAnchor.UpperLeft);
            Note.wordWrap = true;
            Note.normal.background = _highlight;
            Note.padding = new RectOffset(8, 8, 4, 4);
            Button = new GUIStyle(GUI.skin.button) { fontSize = fontSize, fontStyle = FontStyle.Bold };
        }

        public void Dispose()
        {
            if (_dark != null) Object.Destroy(_dark);
            if (_highlight != null) Object.Destroy(_highlight);
        }

        private static GUIStyle Make(int size, FontStyle style, TextAnchor anchor)
        {
            var made = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = style, alignment = anchor };
            made.normal.textColor = Color.white;
            return made;
        }

        private static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
