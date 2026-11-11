using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>Builds the one tiny pixel-art star the sparkle trail uses (white, so each sparkle can be tinted). The caller destroys the texture.</summary>
    public static class SparkleSprite
    {
        private const int Size = 5;
        private const float PixelsPerUnit = 32f;

        /// <summary>Creates the star texture and its sprite.</summary>
        public static Sprite Create(out Texture2D texture)
        {
            texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "ValetSparkle" };
            for (var y = 0; y < Size; y++)
                for (var x = 0; x < Size; x++) texture.SetPixel(x, y, IsLit(x, y) ? Color.white : Color.clear);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), PixelsPerUnit);
        }

        /// <summary>True when the pixel is part of the star: the middle row and column and the centre block.</summary>
        public static bool IsLit(int x, int y)
        {
            var mid = Size / 2;
            return x == mid || y == mid || (Mathf.Abs(x - mid) == 1 && Mathf.Abs(y - mid) == 1);
        }
    }
}
