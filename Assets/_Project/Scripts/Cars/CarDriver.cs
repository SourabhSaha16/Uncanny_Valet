using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Top-down car movement on a Rigidbody2D (gravity 0). Input only counts while driven.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class CarDriver : MonoBehaviour, IDrivable
    {
        [SerializeField] private CarDefinition _definition;

        private Rigidbody2D _body;
        private CarId _id;
        private float _throttle;
        private float _steer;

        public CarId Id => _id;
        public Transform Transform => transform;
        public float Speed => _body != null ? _body.linearVelocity.magnitude : 0f;
        public bool HasDriver { get; private set; }
        public CarDefinition Definition => _definition;

        public void Initialize(CarId id, CarDefinition definition)
        {
            _id = id;
            _definition = definition;
            EnsureBody();
        }

        public void SetInput(float throttle, float steer)
        {
            _throttle = Mathf.Clamp(throttle, -1f, 1f);
            _steer = Mathf.Clamp(steer, -1f, 1f);
        }

        public void SetDriver(bool hasDriver)
        {
            if (HasDriver == hasDriver) return;
            HasDriver = hasDriver;
            if (!hasDriver) { _throttle = 0f; _steer = 0f; }
            ApplyDrag();
            if (!Services.TryGet<IEventBus>(out var bus)) return;
            if (hasDriver) bus.Publish(new CarEntered(_id));
            else bus.Publish(new CarExited(_id));
        }

        private void Awake() => EnsureBody();

        private void EnsureBody()
        {
            if (_body == null) _body = GetComponent<Rigidbody2D>();
            _body.gravityScale = 0f;
            if (_definition != null) _body.mass = _definition.Mass;
            ApplyDrag();
        }

        // Driven: the velocity maths in FixedUpdate is the only damping. Parked: heavy drag as well.
        private void ApplyDrag()
        {
            if (_body == null || _definition == null) return;
            _body.linearDamping = HasDriver ? 0f : _definition.ParkedLinearDrag;
            _body.angularDamping = HasDriver ? 0f : _definition.ParkedAngularDrag;
        }

        private void FixedUpdate()
        {
            if (_definition == null) return;
            float dt = Time.fixedDeltaTime;
            Vector2 up = transform.up, right = transform.right;
            Vector2 velocity = _body.linearVelocity;
            float forward = Vector2.Dot(velocity, up);
            float side = Vector2.Dot(velocity, right);

            float throttle = HasDriver ? _throttle : 0f;
            float steer = HasDriver ? _steer : 0f;
            forward = CarPhysicsMath.NextForwardSpeed(forward, throttle, _definition.Acceleration,
                _definition.Braking, _definition.CoastDrag, _definition.TopSpeed,
                _definition.ReverseFactor, dt);
            side = CarPhysicsMath.DampSideways(side, _definition.Grip, dt);

            _body.linearVelocity = up * forward + right * side;
            _body.angularVelocity = CarPhysicsMath.TurnDegreesPerSecond(
                steer, forward, _definition.TurnRate, _definition.TopSpeed);
        }
    }
}
