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
        private bool _drift;
        private CarHandlingLive _handling;

        public CarId Id => _id;
        public Transform Transform => transform;
        public float Speed => _body != null ? _body.linearVelocity.magnitude : 0f;
        public bool HasDriver { get; private set; }
        public CarDefinition Definition => _definition;

        /// <summary>The velocity set in the last FixedUpdate, i.e. before the physics step (collisions read the pre-impact speed from it).</summary>
        public Vector2 PreSolveVelocity { get; private set; }

        /// <summary>Input editors read before the handling model; faults add themselves here.</summary>
        public DriveModifierStack Modifiers { get; } = new DriveModifierStack();

        /// <param name="handling">Live handling numbers shared by all cars of the factory.</param>
        public void Initialize(CarId id, CarDefinition definition, CarHandlingLive handling = null)
        {
            _id = id;
            _definition = definition;
            _handling = handling;
            EnsureBody();
        }

        public void SetInput(float throttle, float steer)
        {
            _throttle = Mathf.Clamp(throttle, -1f, 1f);
            _steer = Mathf.Clamp(steer, -1f, 1f);
        }

        public void SetDrift(bool drift) => _drift = drift;

        public void SetDriver(bool hasDriver)
        {
            if (HasDriver == hasDriver) return;
            HasDriver = hasDriver;
            if (!hasDriver) { _throttle = 0f; _steer = 0f; _drift = false; }
            ApplyDrag();
            if (!Services.TryGet<IEventBus>(out var bus)) return;
            if (hasDriver) bus.Publish(new CarEntered(_id));
            else bus.Publish(new CarExited(_id));
        }

        private void Awake() => EnsureBody();

        // Despawned while someone sits in it: tell the seated valet so it can get out (no double publish).
        private void OnDestroy() => ReleaseDriverOnDestroy();

        /// <summary>Publishes CarExited once if a driver is seated. Public so EditMode tests can call it.</summary>
        public void ReleaseDriverOnDestroy()
        {
            if (!HasDriver) return;
            HasDriver = false;
            if (Services.TryGet<IEventBus>(out var bus)) bus.Publish(new CarExited(_id));
        }

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

        // Order: input modifiers, drift model (velocity lags the facing), throttle/brake along the facing, steering.
        private void FixedUpdate()
        {
            if (_definition == null) return;
            if (_handling == null) _handling = FallbackHandling();
            var input = HasDriver ? Modifiers.Apply(new DriveInput(_throttle, _steer, _drift)) : default;
            Vector2 up = transform.up;
            var velocity = DriftModel.Step(_body.linearVelocity, up, input.Drift, _handling.Drift, Time.fixedDeltaTime);
            var forward = Vector2.Dot(velocity, up);
            var next = CarPhysicsMath.NextForwardSpeed(forward, input.Throttle, _definition.Acceleration,
                _definition.Braking, _definition.CoastDrag, _definition.TopSpeed, _definition.ReverseFactor, Time.fixedDeltaTime);
            _body.linearVelocity = PreSolveVelocity = velocity + up * (next - forward);
            _body.angularVelocity = Mathf.Rad2Deg
                * SteeringModel.TurnRate(input.Steer, next, _handling.MaxTurnRate, _handling.TurnFullSpeed);
        }

        private CarHandlingLive FallbackHandling()
        {
            Debug.LogError("Car " + name + " has no handling config; using defaults. Rebuild with Setup/Build Everything.", this);
            return new CarHandlingLive(ScriptableObject.CreateInstance<CarHandlingConfig>());
        }
    }
}
