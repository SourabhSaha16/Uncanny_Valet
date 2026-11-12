using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// One car's damage meter. A hard hit on a wall, a station edge or another car (parked and queued cars included, so both cars of a crash
    /// are hurt) adds damage by <see cref="DamageRule"/> and publishes <see cref="CarImpact"/> and <see cref="CarDamaged"/>.
    /// Pedestrians and the valet on foot never damage a car, and a tap below the threshold is free. The meter resets when a shift starts.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(CarDriver))]
    public sealed class CarDamage : MonoBehaviour
    {
        private Rigidbody2D _body;
        private CarDriver _driver;
        private CarHandlingLive _live;
        private IEventBus _subscribedBus;

        /// <summary>Damage so far, 0..1 (1 = wrecked).</summary>
        public float Damage01 { get; private set; }

        /// <summary>Wires the live damage numbers (the factory calls this right after adding the component).</summary>
        public void Initialize(CarHandlingLive live)
        {
            _live = live;
            _body = GetComponent<Rigidbody2D>();
            _driver = GetComponent<CarDriver>();
        }

        /// <summary>Back to undamaged.</summary>
        public void ResetDamage() => Damage01 = 0f;

        private void OnEnable()
        {
            if (!Services.TryGet<IEventBus>(out _subscribedBus)) return;
            _subscribedBus.Subscribe<ShiftStarted>(OnShiftStarted);
        }

        private void OnDisable()
        {
            _subscribedBus?.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _subscribedBus = null;
        }

        private void OnShiftStarted(ShiftStarted e) => ResetDamage();

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_live == null || !CountsAsHit(collision)) return;
            var impulse = 0f;
            for (var i = 0; i < collision.contactCount; i++) impulse += collision.GetContact(i).normalImpulse;
            var force = DamageRule.Force(impulse, _body.mass);
            if (force <= _live.DamageMinForce || !Services.TryGet<IEventBus>(out var bus)) return;
            bus.Publish(new CarImpact(_driver.Id, force, _driver.HasDriver));
            var delta = DamageRule.Add(Damage01, DamageRule.Delta01(force, _live.DamageMinForce, _live.DamagePercentPerForce)) - Damage01;
            if (delta <= 0f) return;
            Damage01 += delta;
            bus.Publish(new CarDamaged(_driver.Id, Damage01, delta));
        }

        // Walls and station edges have no body; another car counts; anything else with a body (the valet, props) does not.
        private static bool CountsAsHit(Collision2D collision)
        {
            if (collision.collider.gameObject.layer == PhysicsLayers.Pedestrian) return false;
            return collision.rigidbody == null || collision.rigidbody.GetComponent<CarDriver>() != null;
        }
    }
}
