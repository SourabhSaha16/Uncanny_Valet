using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Draws the mud puddle sprites in code, once, and hands them out; nothing is drawn per frame. Each sprite is one unit wide
    /// (scale it to the puddle's size), so every puddle keeps the same chunky pixels whatever its width.
    /// </summary>
    public sealed class MudBlobArt
    {
        /// <summary>Pixels across one puddle picture.</summary>
        public const int Pixels = 24;

        private static readonly Color32 Body = new Color32(74, 48, 28, 255);
        private static readonly Color32 Rim = new Color32(122, 86, 52, 255);
        private static readonly Color32 Ripple = new Color32(98, 66, 40, 255);

        private readonly Sprite[] _sprites = new Sprite[MudBlobPixels.Variants];

        /// <summary>The cached sprite of a variant (wraps around the variant count).</summary>
        public Sprite Get(int variant)
        {
            var index = ((variant % _sprites.Length) + _sprites.Length) % _sprites.Length;
            return _sprites[index] != null ? _sprites[index] : _sprites[index] = Draw(index);
        }

        private static Sprite Draw(int variant)
        {
            var texture = new Texture2D(Pixels, Pixels, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[Pixels * Pixels];
            for (var y = 0; y < Pixels; y++)
            for (var x = 0; x < Pixels; x++) pixels[y * Pixels + x] = Colour(MudBlobPixels.Sample(x, y, Pixels, variant));
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, Pixels, Pixels), new Vector2(0.5f, 0.5f), Pixels);
        }

        private static Color32 Colour(MudTone tone)
        {
            switch (tone)
            {
                case MudTone.Body: return Body;
                case MudTone.Rim: return Rim;
                case MudTone.Ripple: return Ripple;
                default: return new Color32(0, 0, 0, 0);
            }
        }
    }
}
