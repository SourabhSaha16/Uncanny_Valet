using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>Builds the 1x1 white sprite used for coloured-rectangle placeholder art.</summary>
    internal static class PlaceholderSprite
    {
        public static Sprite Create()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
