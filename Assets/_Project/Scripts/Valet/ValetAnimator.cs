using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// Animates the layered valet sprites (body, outfit, hair) from the Rigidbody2D velocity: picks the
    /// facing, runs the 6-frame cycle while moving and shows the idle frame when still. No allocations.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class ValetAnimator : MonoBehaviour
    {
        [SerializeField] private ValetArtConfig _config;
        [SerializeField] private SpriteRenderer _model;
        [SerializeField] private SpriteRenderer _outfit;
        [SerializeField] private SpriteRenderer _hair;

        private Rigidbody2D _body;
        private ValetFacing _facing = ValetFacing.Down;
        private float _movingSeconds;
        private bool _valid;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _valid = _config != null && _config.IsComplete && _model != null && _outfit != null && _hair != null;
            if (!_valid) Debug.LogError("ValetAnimator on " + name + " is missing its config or renderers; rebuild with Setup/Build Everything.", this);
        }

        private void Update()
        {
            if (!_valid) return;
            var velocity = _body.linearVelocity;
            var threshold = _config.MoveThreshold;
            var moving = velocity.sqrMagnitude >= threshold * threshold;
            _facing = ValetAnimMath.Facing(velocity, _facing, threshold);
            _movingSeconds = moving ? _movingSeconds + Time.deltaTime : 0f;
            var frame = moving
                ? ValetAnimMath.Frame(_movingSeconds, _config.FramesPerSecond, _config.FramesPerGroup)
                : _config.IdleFrame;
            Show(ValetAnimMath.Column(_config.GroupStart(_facing), frame), _config.Flip(_facing));
        }

        private void Show(int column, bool flip)
        {
            _model.sprite = _config.Model(column);
            _outfit.sprite = _config.Outfit(column);
            _hair.sprite = _config.Hair(column);
            _model.flipX = flip;
            _outfit.flipX = flip;
            _hair.flipX = flip;
        }
    }
}
