using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>A soft golden disc under the valet or his car that pulses with the Thunder stride glow. One cached sprite, one object.</summary>
    public sealed class AuraGlow : System.IDisposable
    {
        private const int TextureSize = 32;
        private const float SwellBase = 0.85f;
        private const float SwellRange = 0.15f;
        private readonly SpriteRenderer _renderer;
        private readonly Texture2D _texture;
        private readonly Sprite _sprite;
        private float _size = 1f;

        public AuraGlow()
        {
            _texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var half = (TextureSize - 1) * 0.5f;
            for (var y = 0; y < TextureSize; y++)
                for (var x = 0; x < TextureSize; x++)
                {
                    var fall = 1f - Mathf.Clamp01(new Vector2(x - half, y - half).magnitude / half);
                    _texture.SetPixel(x, y, new Color(1f, 1f, 1f, fall * fall));
                }
            _texture.Apply();
            _sprite = Sprite.Create(_texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), TextureSize);
            var go = new GameObject("Thunder glow");
            _renderer = go.AddComponent<SpriteRenderer>();
            _renderer.sprite = _sprite;
            _renderer.enabled = false;
        }

        /// <summary>Sizes and layers the glow for a target: as wide as its sprites times <paramref name="scale"/>, just under the lowest one.</summary>
        public void Bind(SpriteRenderer[] sprites, float scale)
        {
            _size = 1f;
            if (sprites.Length == 0) return;
            var bounds = sprites[0].bounds;
            var order = sprites[0].sortingOrder;
            foreach (var s in sprites)
            {
                bounds.Encapsulate(s.bounds);
                order = Mathf.Min(order, s.sortingOrder);
            }
            _size = Mathf.Max(bounds.size.x, bounds.size.y) * scale;
            _renderer.sortingLayerID = sprites[0].sortingLayerID;
            _renderer.sortingOrder = order - 1;
        }

        /// <summary>Puts the glow at <paramref name="position"/> with the given colour; its size swells with <paramref name="strength"/>.</summary>
        public void Show(Vector2 position, Color color, float strength)
        {
            _renderer.enabled = strength > 0f;
            color.a *= strength;
            _renderer.color = color;
            _renderer.transform.position = position;
            _renderer.transform.localScale = Vector3.one * (_size * (SwellBase + SwellRange * strength));
        }

        /// <summary>Hides the glow.</summary>
        public void Hide() => _renderer.enabled = false;

        /// <summary>Destroys the glow and its sprite.</summary>
        public void Dispose()
        {
            if (_renderer != null) Object.Destroy(_renderer.gameObject);
            if (_sprite != null) Object.Destroy(_sprite);
            if (_texture != null) Object.Destroy(_texture);
        }
    }
}
