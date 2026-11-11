using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>Walks the valet with a Rigidbody2D, faster while the speed boost runs. Does nothing while disabled (driving).</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(ValetBoost))]
    public sealed class OnFootMover : MonoBehaviour
    {
        [SerializeField] private float _speed = 4f;

        private Rigidbody2D _body;
        private ValetBoost _boost;
        private IInputSource _input;

        public void SetInput(IInputSource input) => _input = input;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _boost = GetComponent<ValetBoost>();
        }

        private void FixedUpdate()
        {
            if (_input == null) return;
            _body.linearVelocity = _input.Move * (_speed * _boost.Multiplier);
        }

        private void OnDisable()
        {
            if (_body != null) _body.linearVelocity = Vector2.zero;
        }
    }
}
