using UnityEngine;

namespace UncannyValet.UI.Kit
{
    /// <summary>Builds small Point-filtered textures from code tables. Each texture is made once and cached by its owner.</summary>
    public static class PixelBitmap
    {
        /// <summary>Makes a texture from rows of palette keys, top row first. Unknown keys and '.' are clear.</summary>
        public static Texture2D FromRows(string name, string[] rows, string keys, Color32[] colors)
        {
            var height = rows.Length;
            var width = rows[0].Length;
            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var key = keys.IndexOf(rows[y][x]);
                    if (key >= 0) pixels[(height - 1 - y) * width + x] = colors[key];
                }
            }
            return Make(name, width, height, pixels);
        }

        /// <summary>Makes a texture from raw pixels (bottom row first, as Unity stores them).</summary>
        public static Texture2D Make(string name, int width, int height, Color32[] pixels)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }
    }
}
