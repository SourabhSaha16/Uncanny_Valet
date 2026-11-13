using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// The valet's part of Thunder stride. On foot it dashes in a straight line (the held move direction, else the facing),
    /// passes pedestrians for the dash and stops at solid things. In a car it only draws the lightning trail (the Cars module
    /// does the boost). Both uses publish ThunderStrideStarted for the camera kick.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(OnFootMover), typeof(SeatController))]
    public sealed class ThunderDash : MonoBehaviour
    {
        [SerializeField, Min(0.1f), Tooltip("Spacing of the lightning streaks along the path, in world units.")]
        private float _streakSpacing = 0.7f;

        private readonly DashRule _rule = new DashRule();
        private Rigidbody2D _body;
        private OnFootMover _mover;
        private SeatController _seat;
        private ValetAnimator _animator;
        private IInputSource _input;
        private IEventBus _bus;
        private ThunderStreaks _streaks;
        private Vector2 _lastBody;
        private Vector2 _lastStreak;
        private bool _trailing;
        private bool _ignoringPedestrians;
        private bool _hadIgnore;
        private CollisionDetectionMode2D _mode;

        /// <summary>True while the on-foot dash moves the valet.</summary>
        public bool IsDashing => _rule.IsActive;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _mover = GetComponent<OnFootMover>();
            _seat = GetComponent<SeatController>();
            _animator = GetComponent<ValetAnimator>();
            _input = GetComponent<IInputSource>();
        }

        private void Start()
        {
            _bus = Services.Get<IEventBus>();
            _streaks = new ThunderStreaks(null);
            _bus.Subscribe<PowerActivated>(OnActivated);
            _bus.Subscribe<PowerEnded>(OnEnded);
        }

        private void OnDestroy()
        {
            if (_bus != null)
            {
                _bus.Unsubscribe<PowerActivated>(OnActivated);
                _bus.Unsubscribe<PowerEnded>(OnEnded);
            }
            StopDash();
            _streaks?.Dispose();
        }

        private void OnDisable() => StopDash();

        private void OnActivated(PowerActivated e)
        {
            if (e.Power != StoryPower.ThunderStride) return;
            if (!Services.TryGet<IPowerView>(out var view) || !view.TryGetValues(e.Power, e.Level, out var values)) return;
            _lastStreak = _seat.IsDriving ? (Vector2)transform.position : _body.position;
            if (_seat.IsDriving) { StartTrail(); return; }
            if (!_mover.isActiveAndEnabled) return;
            var facing = _animator != null ? _animator.Facing : ValetFacing.Down;
            var direction = DashDirection.Choose(_input != null ? _input.Move : Vector2.zero, facing);
            BeginDash(direction, values);
            _bus.Publish(new ThunderStrideStarted(_body.position, direction, false));
        }

        private void OnEnded(PowerEnded e)
        {
            if (e.Power != StoryPower.ThunderStride) return;
            _trailing = false;
            StopDash();
        }

        private void StartTrail()
        {
            _trailing = true;
            var car = _seat.Car;
            if (car != null && car.Transform != null) _bus.Publish(new ThunderStrideStarted(car.Transform.position, car.Transform.up, true));
        }

        private void BeginDash(Vector2 direction, PowerLevelValues values)
        {
            _rule.Begin(direction, values.Distance, values.Secondary);
            _lastBody = _body.position;
            _hadIgnore = Physics2D.GetIgnoreLayerCollision(gameObject.layer, PhysicsLayers.Pedestrian);
            Physics2D.IgnoreLayerCollision(gameObject.layer, PhysicsLayers.Pedestrian, true);
            _ignoringPedestrians = true;
            _mode = _body.collisionDetectionMode;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void StopDash()
        {
            _rule.End();
            if (_mover != null) _mover.EndDash();
            if (!_ignoringPedestrians) return;
            _ignoringPedestrians = false;
            _body.linearVelocity = Vector2.zero; // walking takes over from the next step; no leftover dash step
            Physics2D.IgnoreLayerCollision(gameObject.layer, PhysicsLayers.Pedestrian, _hadIgnore);
            _body.collisionDetectionMode = _mode;
        }

        // The mover may already have run this step: set the body now so the first dash step is not a walking step.
        private void Push(Vector2 velocity)
        {
            _mover.SetDash(velocity);
            _body.linearVelocity = velocity;
        }

        private void FixedUpdate()
        {
            var position = _trailing ? (Vector2)transform.position : _body.position;
            if (_rule.IsActive)
            {
                var velocity = _rule.Step(Time.fixedDeltaTime, (position - _lastBody).magnitude);
                _lastBody = position;
                if (_rule.IsActive) Push(velocity); else StopDash();
            }
            if ((_rule.IsActive || _trailing) && (position - _lastStreak).sqrMagnitude >= _streakSpacing * _streakSpacing)
            {
                _streaks.Emit(_lastStreak, position, Time.unscaledTime);
                _lastStreak = position;
            }
        }

        private void Update() => _streaks?.Tick(Time.unscaledTime);
    }
}
