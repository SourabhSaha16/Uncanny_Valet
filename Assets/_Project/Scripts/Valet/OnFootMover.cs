using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>Walks the valet with a Rigidbody2D. Does nothing while disabled (driving).</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class OnFootMover : MonoBehaviour
    {
        /// <summary>The shipped walking speed, units per second.</summary>
        public const float DefaultSpeed = 4f;

        [SerializeField, Min(0.1f)] private float _speed = DefaultSpeed;

        private Rigidbody2D _body;
        private IInputSource _input;
        private WalkSpeedBoost _boost;
        private bool _dashing;
        private Vector2 _dashVelocity;

        public void SetInput(IInputSource input) => _input = input;

        /// <summary>Sets the source of the walking speed multiplier (the Thunder stride boost); null means no boost.</summary>
        public void SetSpeedBoost(WalkSpeedBoost boost) => _boost = boost;

        /// <summary>Overrides walking with a dash velocity until <see cref="EndDash"/>.</summary>
        public void SetDash(Vector2 velocity)
        {
            _dashing = true;
            _dashVelocity = velocity;
        }

        /// <summary>Goes back to walking.</summary>
        public void EndDash() => _dashing = false;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // the Thunder stride run is faster than a dash step
        }

        private void FixedUpdate()
        {
            if (_dashing) { _body.linearVelocity = _dashVelocity; return; }
            if (_input == null) return;
            _body.linearVelocity = _input.Move * (_speed * (_boost != null ? _boost.Multiplier : 1f));
        }

        private void OnDisable()
        {
            _dashing = false;
            if (_body != null) _body.linearVelocity = Vector2.zero;
        }
    }
}
