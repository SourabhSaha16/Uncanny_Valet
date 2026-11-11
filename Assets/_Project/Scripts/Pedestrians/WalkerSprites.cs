using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>The three stacked sprite layers of one person (body, outfit, hair).</summary>
    public sealed class WalkerSprites
    {
        private readonly PedestrianConfig _config;
        private readonly SpriteRenderer _model;
        private readonly SpriteRenderer _outfit;
        private readonly SpriteRenderer _hair;

        public WalkerSprites(Transform parent, PedestrianConfig config)
        {
            _config = config;
            _model = Layer(parent, "Model", config.SortingOrder);
            _outfit = Layer(parent, "Outfit", config.SortingOrder + 1);
            _hair = Layer(parent, "Hair", config.SortingOrder + 2);
        }

        /// <summary>Rotates every layer to <paramref name="degrees"/> around the person (0 = upright).</summary>
        public void Spin(float degrees)
        {
            var rotation = Quaternion.Euler(0f, 0f, degrees);
            _model.transform.localRotation = rotation;
            _outfit.transform.localRotation = rotation;
            _hair.transform.localRotation = rotation;
        }

        /// <summary>Shows sheet <paramref name="column"/> of this look on every layer.</summary>
        public void Show(PedestrianLook look, int column, bool flip)
        {
            Set(_model, _config.Model(look, column), flip);
            Set(_outfit, _config.Outfit(look, column), flip);
            Set(_hair, _config.Hair(look, column), flip);
        }

        private static void Set(SpriteRenderer layer, Sprite sprite, bool flip)
        {
            layer.sprite = sprite;
            layer.flipX = flip;
        }

        private static SpriteRenderer Layer(Transform parent, string layerName, int order)
        {
            var go = new GameObject(layerName);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = order;
            return renderer;
        }
    }
}
