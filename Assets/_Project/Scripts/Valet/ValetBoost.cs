using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// The valet's side of the level 1 speed boost: reads IPowerBoost (registered by the Powers module) for the walk
    /// multiplier, and while boosted lets the valet run through pedestrians (the body ignores the pedestrian layer)
    /// and tells the pedestrians they were barged. Only acts while the valet is on foot.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public sealed class ValetBoost : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("World units added to the body radius when looking for people to barge.")]
        private float _bargeMargin = 0.15f;
        [SerializeField, Min(0.1f), Tooltip("Seconds before the same person can be barged again.")]
        private float _bargeCooldown = 1f;
        [SerializeField, Range(1, 8), Tooltip("How many recently barged people are remembered.")]
        private int _remembered = 8;

        private CircleCollider2D _collider;
        private Rigidbody2D _body;
        private IPowerBoost _boost;
        private BargeScanner _scanner;
        private int _baseExclude;
        private float _radius;
        private bool _excluding;

        /// <summary>True while the speed boost runs (false before the Powers module registers it).</summary>
        public bool IsBoosted => _boost != null && _boost.IsBoosted;

        /// <summary>Factor on walking speed: 1 when not boosted.</summary>
        public float Multiplier => _boost == null ? 1f : BoostMath.Multiplier(_boost.IsBoosted, _boost.WalkMultiplier);

        /// <summary>Seconds of boost left (0 when not boosted).</summary>
        public float Remaining => IsBoosted ? _boost.Remaining : 0f;

        private void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
            _body = GetComponent<Rigidbody2D>();
            _baseExclude = _collider.excludeLayers.value;
            var scale = transform.lossyScale;
            _radius = _collider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)) + _bargeMargin;
        }

        private void Start() => _scanner = new BargeScanner(Services.Get<IEventBus>(), new BargeRule(_remembered, _bargeCooldown));

        private void FixedUpdate()
        {
            if (_boost == null && !Services.TryGet(out _boost)) return;
            var boosted = _boost.IsBoosted;
            SetExcluding(boosted);
            if (boosted && _collider.enabled) _scanner.Scan(_body.position, _radius, Time.time);
        }

        // While boosted the body no longer collides with pedestrians (they walk through, the scan makes them stumble).
        private void SetExcluding(bool excluding)
        {
            if (excluding == _excluding) return;
            _excluding = excluding;
            _collider.excludeLayers = excluding ? _baseExclude | PhysicsLayers.PedestrianMask : _baseExclude;
        }
    }
}
