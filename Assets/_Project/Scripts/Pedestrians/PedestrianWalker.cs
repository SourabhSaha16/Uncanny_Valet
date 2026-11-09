using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// One pedestrian: three stacked sprite layers walking a route back and forth. Scenery only: no collider, no body,
    /// so nothing reacts to it. Moves the transform directly (no physics) and allocates nothing per frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PedestrianWalker : MonoBehaviour
    {
        private PedestrianConfig _config;
        private PedestrianLook _look;
        private RouteWalker _route;
        private SpriteRenderer _model;
        private SpriteRenderer _outfit;
        private SpriteRenderer _hair;
        private WalkFacing _facing = WalkFacing.Down;
        private float _speed;
        private float _walkSeconds;
        private int _shownColumn = -1;
        private bool _shownFlip;

        /// <summary>The look this pedestrian wears.</summary>
        public PedestrianLook Look => _look;

        /// <summary>Builds the three sprite layers and places the walker at <paramref name="startDistance"/> along its route.</summary>
        public void Setup(PedestrianConfig config, PedestrianLook look, Vector2[] route, float speed, float startDistance)
        {
            _config = config;
            _look = look;
            _speed = speed;
            _route = new RouteWalker(route);
            _route.StartAt(startDistance);
            transform.localScale = new Vector3(config.Scale, config.Scale, 1f);
            _model = Layer("Model", config.SortingOrder);
            _outfit = Layer("Outfit", config.SortingOrder + 1);
            _hair = Layer("Hair", config.SortingOrder + 2);
            transform.position = _route.Position;
            Show();
        }

        private SpriteRenderer Layer(string layerName, int order)
        {
            var go = new GameObject(layerName);
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = order;
            return renderer;
        }

        private void Update()
        {
            if (_route == null || !_route.CanWalk) return;
            _route.Advance(_speed * Time.deltaTime);
            _walkSeconds += Time.deltaTime;
            transform.position = _route.Position;
            Show();
        }

        private void Show()
        {
            _facing = WalkAnimMath.Facing(_route.Heading, _facing);
            var frame = WalkAnimMath.Frame(_walkSeconds, _config.FramesPerSecond, _config.FramesPerGroup);
            var column = _config.GroupStart(_facing) + frame;
            var flip = _config.Flip(_facing);
            if (column == _shownColumn && flip == _shownFlip) return;
            _shownColumn = column;
            _shownFlip = flip;
            _model.sprite = _config.Model(_look, column);
            _outfit.sprite = _config.Outfit(_look, column);
            _hair.sprite = _config.Hair(_look, column);
            _model.flipX = flip;
            _outfit.flipX = flip;
            _hair.flipX = flip;
        }
    }
}
