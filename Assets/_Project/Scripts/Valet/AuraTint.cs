using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// Tints the sprites of one target (the valet or the car he drives) through a shared MaterialPropertyBlock, on top of their own
    /// colour, and takes the tint off again. The renderers are cached when the target is bound, so tinting allocates nothing.
    /// </summary>
    public sealed class AuraTint
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        private SpriteRenderer[] _renderers = new SpriteRenderer[0];

        /// <summary>The cached sprites of the bound target.</summary>
        public SpriteRenderer[] Renderers => _renderers;

        /// <summary>Starts tinting a new target; the old one is cleaned first. Null releases everything.</summary>
        public void Bind(Transform target)
        {
            Clear();
            _renderers = target != null ? target.GetComponentsInChildren<SpriteRenderer>(true) : new SpriteRenderer[0];
        }

        /// <summary>Multiplies the sprites by <paramref name="color"/>.</summary>
        public void Apply(Color color)
        {
            _block.SetColor(ColorId, color);
            foreach (var r in _renderers)
                if (r != null) r.SetPropertyBlock(_block);
        }

        /// <summary>Removes the tint from every sprite.</summary>
        public void Clear()
        {
            foreach (var r in _renderers)
                if (r != null) r.SetPropertyBlock(null);
        }
    }
}
