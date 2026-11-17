using System;
using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.UI.Pie
{
    /// <summary>Hollow white rectangle sprites (a bay outline), generated once per bay size, point-filtered and tinted by renderer colour.</summary>
    public sealed class BayBorderSprites : IDisposable
    {
        private readonly int _thickness;
        private readonly Dictionary<long, Sprite> _sprites = new Dictionary<long, Sprite>();
        private readonly List<Texture2D> _textures = new List<Texture2D>();

        public BayBorderSprites(int thicknessPixels) { _thickness = thicknessPixels; }

        /// <summary>The border sprite for a bay of this size in world units, built on first use.</summary>
        public Sprite Get(Vector2 sizeUnits)
        {
            var w = Mathf.Max(2 * _thickness + 1, Mathf.RoundToInt(sizeUnits.x * PickupPieSprites.PixelsPerUnit));
            var h = Mathf.Max(2 * _thickness + 1, Mathf.RoundToInt(sizeUnits.y * PickupPieSprites.PixelsPerUnit));
            var key = ((long)w << 32) | (uint)h;
            if (_sprites.TryGetValue(key, out var sprite)) return sprite;
            sprite = Build(w, h);
            _sprites.Add(key, sprite);
            return sprite;
        }

        public void Dispose()
        {
            foreach (var pair in _sprites) PieObjects.Release(pair.Value);
            for (var i = 0; i < _textures.Count; i++) PieObjects.Release(_textures[i]);
            _sprites.Clear();
            _textures.Clear();
        }

        private Sprite Build(int w, int h)
        {
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "BayBorder" };
            var pixels = new Color32[w * h];
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                    if (x < _thickness || y < _thickness || x >= w - _thickness || y >= h - _thickness) pixels[y * w + x] = Color.white;
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            _textures.Add(texture);
            return Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), PickupPieSprites.PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }
    }
}
