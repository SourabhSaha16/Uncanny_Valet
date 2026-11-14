using System.Collections.Generic;
using UncannyValet.Core.Art;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Places the props of a lot: one object per placement under a "Props" group, one sprite renderer per recipe layer (sprites come from
    /// the per-lot <see cref="PropSpriteCache"/>, so each recipe is drawn once), a solid block for solid recipes, and one
    /// <see cref="PropAnimator"/> for the turning and blinking layers. A recipe that fails its validation is logged and skipped.
    /// </summary>
    public sealed class PropBuilder
    {
        private readonly PropSpriteCache _cache;
        private PropAnimator _animator;

        public PropBuilder(WorldPalette palette) => _cache = new PropSpriteCache(palette);

        /// <summary>Recipes drawn so far.</summary>
        public int GeneratedCount => _cache.GeneratedCount;

        /// <summary>Builds the placements under <paramref name="lot"/>; returns the group, or null when there is nothing to place.</summary>
        public Transform Build(Transform lot, IReadOnlyList<PropPlacement> placements)
        {
            if (placements == null || placements.Count == 0) return null;
            var group = new GameObject("Props").transform;
            group.SetParent(lot, false);
            _animator = group.gameObject.AddComponent<PropAnimator>();
            for (var i = 0; i < placements.Count; i++) Place(group, placements[i]);
            return group;
        }

        private void Place(Transform group, PropPlacement placement)
        {
            var recipe = placement.Recipe;
            if (!PropRecipeRules.TryValidate(recipe, out var problem))
            {
                Debug.LogError("Prop skipped: " + problem, group);
                return;
            }
            var root = new GameObject(recipe.name).transform;
            root.SetParent(group, false);
            root.localPosition = placement.Position;
            var sprites = _cache.Get(recipe);
            if (recipe.SpriteOverride != null) Layer(root, recipe, null, sprites[0], placement.SortingOrder);
            else
                for (var i = 0; i < sprites.Length; i++) Layer(root, recipe, recipe.Layers[i], sprites[i], placement.SortingOrder);
            if (recipe.Solid) root.gameObject.AddComponent<BoxCollider2D>().size = recipe.ColliderSize;
        }

        private void Layer(Transform root, PropRecipe recipe, PropLayer layer, Sprite sprite, int order)
        {
            var go = new GameObject(layer != null ? layer.Name : "Sprite");
            go.transform.SetParent(root, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            if (layer == null) { renderer.sortingOrder = order; return; }
            renderer.sortingOrder = order + layer.OrderOffset;
            if (layer.Glows && recipe.GlowMaterial != null) renderer.sharedMaterial = recipe.GlowMaterial;
            if (layer.Animation == PropAnimation.Spin)
                go.transform.localPosition = (Vector2)(layer.Pivot - recipe.SizePixels / 2) / PropRecipe.PixelsPerUnit;
            _animator.Add(renderer, layer.Animation, layer.Speed);
        }
    }
}
