using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>Walks the valet with a Rigidbody2D. Does nothing while disabled (driving).</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class OnFootMover : MonoBehaviour
    {
        [SerializeField] private float _speed = 4f;

        private Rigidbody2D _body;
        private IInputSource _input;
        private bool _dashing;
        private Vector2 _dashVelocity;

        public void SetInput(IInputSource input) => _input = input;

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
        }

        private void FixedUpdate()
        {
            if (_dashing) { _body.linearVelocity = _dashVelocity; return; }
            if (_input == null) return;
            _body.linearVelocity = _input.Move * _speed;
        }

        private void OnDisable()
        {
            _dashing = false;
            if (_body != null) _body.linearVelocity = Vector2.zero;
        }
    }
}
