using System;
using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// One pooled person: three stacked sprite layers that walk a route and can be hidden. Solid while walking: a circle
    /// collider on a kinematic body (layer <see cref="PhysicsLayers.Pedestrian"/>) moved in FixedUpdate. It looks ahead
    /// and waits when a car, the valet, a wall or another person is in the way (see <see cref="PedestrianBlockRule"/>).
    /// Allocates nothing per frame. The crowd decides what it does next.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public sealed class PedestrianWalker : MonoBehaviour
    {
        private readonly PathFollower _path = new PathFollower();
        private readonly PedestrianBlockRule _block = new PedestrianBlockRule();
        private readonly List<Vector2> _single = new List<Vector2>(1);
        private PedestrianConfig _config;
        private PedestrianLook _look;
        private readonly PedestrianBounceRule _bounce = new PedestrianBounceRule();
        private WalkerBody _body;
        private WalkerStumbler _stumble;
        private WalkerSprites _sprites;
        private WalkFacing _facing = WalkFacing.Down;
        private Vector2 _lookAt;
        private bool _hasLookAt;
        private bool _moving;
        private float _speed;
        private readonly WorldTimeReader _world = new WorldTimeReader();
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

        /// <summary>Raised when a car or the valet bumps this person; the value is the bump strength (0..1).</summary>
        public event Action<PedestrianWalker, float> Bumped;

        /// <summary>Raised when a car knocks this person away: the car, the contact point and the hit speed.</summary>
        public event Action<PedestrianWalker, IDrivable, Vector2, float> CarHit;

        /// <summary>Where the person stands now.</summary>
        public Vector2 Position => transform.position;

        /// <summary>Builds the body, collider and sprite layers. Called once when the crowd creates this person.</summary>
        public void Create(PedestrianConfig config)
        {
            _config = config;
            transform.localScale = new Vector3(config.Scale, config.Scale, 1f);
            _body = new WalkerBody(gameObject, config);
            _bounce.Configure(config.BounceDistance, config.BounceSeconds, config.BounceCooldown);
            _sprites = new WalkerSprites(transform, config);
            _stumble = new WalkerStumbler(config, _sprites);
            _stumble.Started += () => _moving = false;
        }

        /// <summary>Gives the person a new look and walking speed; <paramref name="patience01"/> (0..1) sets how long they wait when blocked.</summary>
        public void Dress(PedestrianLook look, float speed, float patience01 = 0.5f)
        {
            _look = look;
            _speed = speed;
            _block.Configure(_config.WaitFor(patience01), _config.PassSeconds);
            _shownColumn = -1;
        }

        /// <summary>Shows or hides the person (a hidden person does not update).</summary>
        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        /// <summary>Passes through the patron's own car: no collision, push, wait or bump (until <see cref="ReleaseCar"/>).</summary>
        public void BindCar(Transform car) => _body.IgnoreCar(car);

        /// <summary>Stops exempting the own car (the patron despawned).</summary>
        public void ReleaseCar() => _body.ReleaseCar();
        private void OnEnable() => _body?.ReapplyIgnore();

        /// <summary>Knocks the person <paramref name="distance"/> units away along <paramref name="push"/> in a tumble of <paramref name="seconds"/>; the route resumes afterwards. False while already tumbling.</summary>
        public bool TryKnockback(Vector2 push, float distance, float seconds) => _stumble.TryStart(push, distance, seconds);

        /// <summary>Places the person without walking.</summary>
        public void PlaceAt(Vector2 position)
        {
            _single.Clear();
            _single.Add(position);
            _path.Begin(_single);
            _hasLookAt = false;
            _moving = false;
            _body.Teleport(position);
            _body.SetSolid(_config.StandingSolid);
            Show();
        }

        /// <summary>Makes a standing person turn to face <paramref name="point"/> (cleared by the next walk or placement).</summary>
        public void FaceToward(Vector2 point)
        {
            _lookAt = point;
            _hasLookAt = true;
            Show();
        }

        /// <summary>Starts walking a route, <paramref name="startFraction"/> (0..1) of the way in.</summary>
        public void Walk(List<Vector2> route, float startFraction = 0f)
        {
            _path.Begin(route, startFraction);
            _hasLookAt = false; _moving = _path.Walking;
            _block.Reset();
            _body.Teleport(_path.Position);
            _body.SetSolid(true);
            Show();
        }

        // A car or the valet (a dynamic body) ran into this person: a tiny rubber-ball bounce away from it.
        private void OnCollisionEnter2D(Collision2D hit)
        {
            var other = hit.rigidbody;
            if (other == null || other.bodyType != RigidbodyType2D.Dynamic) return;
            var car = hit.collider.GetComponentInParent<IDrivable>();
            if (car != null && OwnCarRule.IsOwn(Car, car.Id)) return;
            var speed = hit.relativeVelocity.magnitude;
            if (car != null)
            {
                if (WalkerKnockback.TryApply(this, other.position, speed, _config.BounceMinSpeed)) CarHit?.Invoke(this, car, other.position, speed);
                return;
            }
            var strength = PedestrianBounceRule.Strength(speed, _config.BounceMinSpeed, _config.BounceFullSpeed);
            if (!_bounce.TryStart((Vector2)transform.position - other.position, strength)) return;
            _moving = false;
            Bumped?.Invoke(this, strength);
        }

        private void Update()
        {
            if (!_moving) return;
            _walkSeconds += Time.deltaTime * _world.Scale; // freeze time stops the stride too
            Show();
        }

        private void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;
            if (_stumble.Active) { _stumble.Step(dt); _body.MoveTo(_path.Position + _stumble.Offset); return; }
            if (_bounce.Active) { _bounce.Step(dt); _body.MoveTo(_path.Position + _bounce.Offset); return; }
            _bounce.Step(dt);
            if (!_path.Walking) return;
            var walkDt = _world.Scaled(dt); // bumps and stumbles stay real time: the player can still push people
            var step = _speed * walkDt;
            var blocked = _path.Remaining > _config.ArrivalRadius && _body.IsBlocked(_path.Heading, step + _config.LookAhead, Car);
            var state = _block.Evaluate(walkDt, blocked);
            _body.SetSolid(state != PedestrianBlockState.Passing);
            _moving = state != PedestrianBlockState.Waiting;
            if (!_moving) { Show(); return; }
            var finished = _path.Advance(step);
            _body.MoveTo(_path.Position);
            if (!finished) return;
            _moving = false;
            _body.SetSolid(_config.StandingSolid);
            Show();
            RouteFinished?.Invoke(this);
        }

        private void Show()
        {
            _facing = _hasLookAt && !_path.Walking
                ? WalkAnimMath.FacingToward(transform.position, _lookAt, _facing)
                : WalkAnimMath.Facing(_path.Heading, _facing);
            var frame = _moving ? WalkAnimMath.Frame(_walkSeconds, _config.FramesPerSecond, _config.FramesPerGroup) : 0;
            var column = _config.GroupStart(_facing) + frame;
            var flip = _config.Flip(_facing);
            if (column == _shownColumn && flip == _shownFlip) return;
            _shownColumn = column;
            _shownFlip = flip;
            _sprites.Show(_look, column, flip);
        }
    }
}
