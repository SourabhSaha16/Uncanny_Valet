using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// The valet's part of Thunder stride, on foot and in a car: it draws the lightning trail behind him while the power
    /// lasts, publishes ThunderStrideStarted for the camera kick, and on foot lends the walking speed multiplier to the
    /// mover (the Cars module does the car boost).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(OnFootMover), typeof(SeatController))]
    public sealed class ThunderDash : MonoBehaviour
    {
        [SerializeField, Min(0.1f), Tooltip("Spacing of the lightning streaks along the path, in world units.")]
        private float _streakSpacing = 0.7f;

        private Rigidbody2D _body;
        private OnFootMover _mover;
        private SeatController _seat;
        private ValetAnimator _animator;
        private IInputSource _input;
        private IEventBus _bus;
        private ThunderStreaks _streaks;
        private WalkSpeedBoost _boost;
        private Vector2 _lastStreak;
        private bool _trailing;

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
            _boost = new WalkSpeedBoost(_bus, () => Services.TryGet<IPowerView>(out var view) ? view : null);
            _mover.SetSpeedBoost(_boost);
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
            _boost?.Dispose();
            _streaks?.Dispose();
        }

        private void OnActivated(PowerActivated e)
        {
            if (e.Power != StoryPower.ThunderStride) return;
            _trailing = true;
            _lastStreak = Position();
            var car = _seat.Car;
            if (car != null && car.Transform != null) { _bus.Publish(new ThunderStrideStarted(car.Transform.position, car.Transform.up, true)); return; }
            var facing = _animator != null ? _animator.Facing : ValetFacing.Down;
            var direction = DashDirection.Choose(_input != null ? _input.Move : Vector2.zero, facing);
            _bus.Publish(new ThunderStrideStarted(_body.position, direction, false));
        }

        private void OnEnded(PowerEnded e)
        {
            if (e.Power == StoryPower.ThunderStride) _trailing = false;
        }

        private Vector2 Position() => _seat.IsDriving ? (Vector2)transform.position : _body.position;

        private void FixedUpdate()
        {
            if (!_trailing) return;
            var position = Position();
            if ((position - _lastStreak).sqrMagnitude < _streakSpacing * _streakSpacing) return;
            _streaks.Emit(_lastStreak, position, Time.unscaledTime);
            _lastStreak = position;
        }

        private void Update() => _streaks?.Tick(Time.unscaledTime);
    }
}
