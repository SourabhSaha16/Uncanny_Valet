using UnityEngine;

namespace UncannyValet.UI
{
    /// <summary>IMGUI styles and textures for the boss panel. Created once, released with Dispose.</summary>
    public sealed class MessagePanelStyles
    {
        public readonly GUIStyle Box;
        public readonly GUIStyle Header;
        public readonly GUIStyle Strikes;
        public readonly GUIStyle Text;
        public readonly GUIStyle Open;
        public readonly GUIStyle Done;
        public readonly GUIStyle Failed;
        public readonly GUIStyle Cancelled;
        public readonly Texture2D White;
        private readonly Texture2D _dark;

        public MessagePanelStyles(int fontSize, float backgroundAlpha)
        {
            _dark = Solid(new Color(0f, 0f, 0f, backgroundAlpha));
            White = Solid(Color.white);
            Box = new GUIStyle(GUI.skin.box) { normal = { background = _dark } };
            Header = Label(fontSize + 2, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            Strikes = Label(fontSize, FontStyle.Bold, TextAnchor.MiddleRight, new Color(1f, 0.45f, 0.35f));
            Text = Label(fontSize, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
            Open = Label(fontSize - 2, FontStyle.Bold, TextAnchor.MiddleRight, new Color(1f, 0.85f, 0.3f));
            Done = Label(fontSize - 2, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.4f, 0.9f, 0.45f));
            Failed = Label(fontSize - 2, FontStyle.Bold, TextAnchor.MiddleRight, new Color(1f, 0.35f, 0.3f));
            Cancelled = Label(fontSize - 2, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.7f, 0.7f, 0.7f));
        }

        public GUIStyle Badge(MessageStatus status)
        {
            switch (status)
            {
                case MessageStatus.Done: return Done;
                case MessageStatus.Failed: return Failed;
                case MessageStatus.Cancelled: return Cancelled;
                default: return Open;
            }
        }

        public void Dispose()
        {
            if (_dark != null) Object.Destroy(_dark);
            if (White != null) Object.Destroy(White);
        }

        private static GUIStyle Label(int size, FontStyle style, TextAnchor anchor, Color color)
        {
            var made = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = style, alignment = anchor };
            made.normal.textColor = color;
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
