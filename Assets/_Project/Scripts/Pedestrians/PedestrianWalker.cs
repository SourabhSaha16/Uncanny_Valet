using System;
using System.Collections.Generic;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// One pooled person: three stacked sprite layers that walk a route and can be hidden. Scenery only: no collider,
    /// no body. Moves the transform directly and allocates nothing per frame. The crowd decides what it does next.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PedestrianWalker : MonoBehaviour
    {
        private readonly PathFollower _path = new PathFollower();
        private readonly List<Vector2> _single = new List<Vector2>(1);
        private PedestrianConfig _config;
        private PedestrianLook _look;
        private SpriteRenderer _model;
        private SpriteRenderer _outfit;
        private SpriteRenderer _hair;
        private WalkFacing _facing = WalkFacing.Down;
        private float _speed;
        private float _walkSeconds;
        private int _shownColumn = -1;
        private bool _shownFlip;

        /// <summary>The state machine that decides this person's next phase.</summary>
        public PedestrianBrain Brain { get; } = new PedestrianBrain();

        /// <summary>The phase whose walk (or stillness) is currently shown; the crowd re-applies when the brain moves on.</summary>
        public PedestrianPhase Applied { get; set; }

        /// <summary>The patron's car, or default for an ambient person.</summary>
        public CarId Car { get; set; }

        /// <summary>The curb standing spot of a patron.</summary>
        public int Slot { get; set; }

        /// <summary>Which road route an ambient person walks (in or out).</summary>
        public int RouteIndex { get; set; }

        /// <summary>How far along the first walk an ambient person starts (opening of the night only).</summary>
        public float StartFraction { get; set; }

        /// <summary>True once the patron's car was delivered (they board it instead of vanishing).</summary>
        public bool Delivered { get; set; }

        /// <summary>Raised once when a walk reaches its end.</summary>
        public event Action<PedestrianWalker> RouteFinished;

        /// <summary>Where the person stands now.</summary>
        public Vector2 Position => transform.position;

        /// <summary>Builds the sprite layers. Called once when the crowd creates this person.</summary>
        public void Create(PedestrianConfig config)
        {
            _config = config;
            transform.localScale = new Vector3(config.Scale, config.Scale, 1f);
            _model = Layer("Model", config.SortingOrder);
            _outfit = Layer("Outfit", config.SortingOrder + 1);
            _hair = Layer("Hair", config.SortingOrder + 2);
        }

        /// <summary>Gives the person a new look and walking speed.</summary>
        public void Dress(PedestrianLook look, float speed)
        {
            _look = look;
            _speed = speed;
            _shownColumn = -1;
        }

        /// <summary>Shows or hides the person (a hidden person does not update).</summary>
        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        /// <summary>Places the person without walking.</summary>
        public void PlaceAt(Vector2 position)
        {
            _single.Clear();
            _single.Add(position);
            _path.Begin(_single);
            transform.position = position;
            Show();
        }

        /// <summary>Starts walking a route, <paramref name="startFraction"/> (0..1) of the way in.</summary>
        public void Walk(List<Vector2> route, float startFraction = 0f)
        {
            _path.Begin(route, startFraction);
            transform.position = _path.Position;
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
            if (!_path.Walking) return;
            var finished = _path.Advance(_speed * Time.deltaTime);
            _walkSeconds += Time.deltaTime;
            transform.position = _path.Position;
            Show();
            if (finished) RouteFinished?.Invoke(this);
        }

        private void Show()
        {
            _facing = WalkAnimMath.Facing(_path.Heading, _facing);
            var moving = _path.Walking;
            var frame = moving ? WalkAnimMath.Frame(_walkSeconds, _config.FramesPerSecond, _config.FramesPerGroup) : 0;
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
