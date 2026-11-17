using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// The free quick dash on foot, on the Dash/Handbrake button (Shift): one short burst in the held move direction (else
    /// the facing), then a cooldown. It only moves the valet, so pedestrians and solid things still block and bump him.
    /// Does nothing in a car (Shift is the handbrake there) or while input is blocked (cards, pause, text field).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(OnFootMover), typeof(SeatController))]
    public sealed class ShiftDash : MonoBehaviour
    {
        /// <summary>The shipped dash distance, world units.</summary>
        public const float DefaultDistance = 3f;
        /// <summary>The shipped dash time, seconds.</summary>
        public const float DefaultSeconds = 0.2f;

        [SerializeField, Min(0.1f), Tooltip("How far one dash goes, in world units.")]
        private float _distance = DefaultDistance;
        [SerializeField, Min(0.05f), Tooltip("How long one dash takes, in seconds.")]
        private float _seconds = DefaultSeconds;
        [SerializeField, Min(0f), Tooltip("Seconds after a dash ends before the next one can start.")]
        private float _cooldown = 0.7f;

        private readonly DashRule _rule = new DashRule();
        private readonly DashCooldown _gate = new DashCooldown();
        private Rigidbody2D _body;
        private OnFootMover _mover;
        private SeatController _seat;
        private ValetAnimator _animator;
        private IInputSource _input;
        private Vector2 _lastBody;
        private CollisionDetectionMode2D _mode;

        /// <summary>True while the dash moves the valet.</summary>
        public bool IsDashing => _gate.IsDashing;

        /// <summary>Seconds until the next dash may start; 0 when ready.</summary>
        public float CooldownRemaining => _gate.Remaining;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _mover = GetComponent<OnFootMover>();
            _seat = GetComponent<SeatController>();
            _animator = GetComponent<ValetAnimator>();
        }

        private void Start()
        {
            // The registered input is the gated one (no input under a card, the pause or a text field).
            if (!Services.TryGet(out _input)) _input = GetComponent<IInputSource>();
            if (_input == null) Debug.LogError("ShiftDash on " + name + " has no IInputSource.", this);
        }

        private void OnDisable() => Cancel();

        private void Update()
        {
            _gate.Tick(Time.deltaTime);
            if (_input == null) return;
            if (_seat.IsDriving) Cancel();
            if (!_gate.TryStart(_input.Drift, _seat.IsDriving)) return;
            var facing = _animator != null ? _animator.Facing : ValetFacing.Down;
            _rule.Begin(DashDirection.Choose(_input.Move, facing), _distance, _seconds);
            _lastBody = _body.position;
            _mode = _body.collisionDetectionMode;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            if (!_rule.IsActive) Finish();
        }

        private void FixedUpdate()
        {
            if (!_rule.IsActive) return;
            var position = _body.position;
            var velocity = _rule.Step(Time.fixedDeltaTime, (position - _lastBody).magnitude);
            _lastBody = position;
            if (!_rule.IsActive) { Finish(); return; }
            // The mover may already have run this step: set the body now so the first dash step is not a walking step.
            _mover.SetDash(velocity);
            _body.linearVelocity = velocity;
        }

        private void Cancel()
        {
            if (_gate.IsDashing) Finish();
        }

        private void Finish()
        {
            _rule.End();
            _mover.EndDash();
            _body.linearVelocity = Vector2.zero; // walking takes over from the next step; no leftover dash step
            _body.collisionDetectionMode = _mode;
            _gate.Finish(_cooldown);
        }
    }
}
