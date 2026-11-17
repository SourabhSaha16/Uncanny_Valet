using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Core.Art
{
    /// <summary>
    /// How to draw one prop in code on the 32 pixel per unit grid with a world palette: size in grid units and layers of shapes (some of
    /// them turning or blinking). When a sprite is assigned it is used instead of the drawing, so an artist can replace props one at a
    /// time without code changes. Read-only at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Art/Prop Recipe", fileName = "PropRecipe")]
    public sealed class PropRecipe : ScriptableObject
    {
        /// <summary>Pixels in one world unit (the MetroCity grid).</summary>
        public const int PixelsPerUnit = 32;
        /// <summary>Largest prop in grid units, either way.</summary>
        public const int MaxUnits = 16;

        [SerializeField, Tooltip("Size of the prop in grid units (one unit is 32 pixels).")]
        private Vector2Int _sizeUnits = new Vector2Int(1, 1);
        [SerializeField] private List<PropLayer> _layers = new List<PropLayer>();
        [SerializeField, Tooltip("Optional hand-made sprite used instead of the code drawing (and its animations). Keep its size equal to the grid size.")]
        private Sprite _spriteOverride;
        [SerializeField, Tooltip("Cars and the valet cannot drive through the prop.")]
        private bool _solid;
        [SerializeField, Tooltip("Size of the solid block in world units (centred on the prop).")]
        private Vector2 _colliderSize;
        [SerializeField, Tooltip("Shared unlit sprite material for glowing layers (set by the art setup tool).")]
        private Material _glowMaterial;

        public Vector2Int SizeUnits => _sizeUnits;
        public Vector2Int SizePixels => _sizeUnits * PixelsPerUnit;
        public IReadOnlyList<PropLayer> Layers => _layers;
        public Sprite SpriteOverride => _spriteOverride;
        public bool Solid => _solid;
        public Vector2 ColliderSize => _colliderSize;
        public Material GlowMaterial => _glowMaterial;

        /// <summary>Checks the asset; on failure <paramref name="problem"/> names the first fault.</summary>
        public bool TryValidate(out string problem) => PropRecipeRules.TryValidate(this, out problem);

        /// <summary>Makes a recipe (used by the setup tool and tests).</summary>
        public static PropRecipe Create(string recipeName, Vector2Int sizeUnits, PropLayer[] layers, bool solid = false,
            Vector2 colliderSize = default, Material glowMaterial = null, Sprite spriteOverride = null)
        {
            var recipe = CreateInstance<PropRecipe>();
            recipe.name = recipeName;
            recipe._sizeUnits = sizeUnits;
            recipe._layers = new List<PropLayer>(layers);
            recipe._solid = solid;
            recipe._colliderSize = colliderSize;
            recipe._glowMaterial = glowMaterial;
            recipe._spriteOverride = spriteOverride;
            return recipe;
        }
    }
}
