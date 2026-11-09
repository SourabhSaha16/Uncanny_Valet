using UnityEngine;

namespace UncannyValet.UI
{
    /// <summary>The IMGUI styles and texture for the results screen. Created once, released with Dispose.</summary>
    public sealed class ResultsStyles
    {
        public readonly GUIStyle Box;
        public readonly GUIStyle Title;
        public readonly GUIStyle Stars;
        public readonly GUIStyle Line;
        public readonly GUIStyle Button;
        private readonly Texture2D _dark;

        public ResultsStyles(int fontSize, float backdropAlpha, int starScale)
        {
            _dark = new Texture2D(1, 1);
            _dark.SetPixel(0, 0, new Color(0f, 0f, 0f, backdropAlpha));
            _dark.Apply();
            Box = new GUIStyle(GUI.skin.box) { normal = { background = _dark } };
            Title = Make(fontSize + 6, FontStyle.Bold);
            Stars = Make(fontSize * starScale, FontStyle.Bold);
            Line = Make(fontSize, FontStyle.Normal);
            Button = new GUIStyle(GUI.skin.button) { fontSize = fontSize, fontStyle = FontStyle.Bold };
        }

        public void Dispose()
        {
            if (_dark != null) Object.Destroy(_dark);
        }

        private static GUIStyle Make(int size, FontStyle style)
        {
            var made = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = style, alignment = TextAnchor.MiddleCenter };
            made.normal.textColor = Color.white;
            return made;
        }
    }
}
