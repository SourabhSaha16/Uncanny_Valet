using System.Collections.Generic;
using UncannyValet.Core.Art;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Draws each recipe once per lot load and hands out the cached sprites (one per layer; a recipe with a sprite override hands out that
    /// sprite and draws nothing). Textures use point filtering at 32 pixels per unit. Nothing is drawn after loading.
    /// </summary>
    public sealed class PropSpriteCache
    {
        private readonly WorldPalette _palette;
        private readonly Dictionary<PropRecipe, Sprite[]> _sprites = new Dictionary<PropRecipe, Sprite[]>();

        public PropSpriteCache(WorldPalette palette) => _palette = palette;

        /// <summary>Recipes drawn so far (an override counts as drawn once too).</summary>
        public int GeneratedCount => _sprites.Count;

        /// <summary>The sprites of a recipe, one per layer (just the override when set), drawn on first request.</summary>
        public Sprite[] Get(PropRecipe recipe)
        {
            if (_sprites.TryGetValue(recipe, out var cached)) return cached;
            var sprites = recipe.SpriteOverride != null ? new[] { recipe.SpriteOverride } : Draw(recipe);
            _sprites.Add(recipe, sprites);
            return sprites;
        }

        private Sprite[] Draw(PropRecipe recipe)
        {
            var sprites = new Sprite[recipe.Layers.Count];
            var size = recipe.SizePixels;
            for (var i = 0; i < sprites.Length; i++)
            {
                var layer = recipe.Layers[i];
                var texture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = recipe.name + " " + layer.Name };
                texture.SetPixels32(PropPixels.Render(recipe, layer, _palette));
                texture.Apply();
                var pivot = layer.Animation == PropAnimation.Spin ? new Vector2(layer.Pivot.x / (float)size.x, layer.Pivot.y / (float)size.y) : new Vector2(0.5f, 0.5f);
                sprites[i] = Sprite.Create(texture, new Rect(0, 0, size.x, size.y), pivot, PropRecipe.PixelsPerUnit, 0, SpriteMeshType.FullRect);
            }
            return sprites;
        }
    }
}
