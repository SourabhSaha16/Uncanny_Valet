using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Rubber bounce between two cars. Applies only when both are live: their CarDriver is enabled and the body is
    /// Dynamic. Cars under CarAutoDrive or a trip (kinematic, collider and driver off) are ignored, so arriving and
    /// queued cars neither trigger nor suffer it. Each car overrides its own speed along the contact normal with the result of
    /// a restitution collision computed from the pre-impact velocities. The car with the lower id publishes <see cref="CarsCollided"/>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CarDriver))]
    public sealed class CarCollisionResponse : MonoBehaviour
    {
        private Rigidbody2D _body;
        private CarDriver _driver;
        private CarHandlingKit _kit;

        /// <summary>Wires the shared handling numbers (the factory calls this right after adding the component).</summary>
        public void Initialize(CarHandlingKit kit)
        {
            _kit = kit;
            _body = GetComponent<Rigidbody2D>();
            _driver = GetComponent<CarDriver>();
        }

        private bool IsLive => _kit != null && _driver.isActiveAndEnabled && _body.bodyType == RigidbodyType2D.Dynamic;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsLive || collision.rigidbody == null || !collision.rigidbody.TryGetComponent(out CarCollisionResponse other) || !other.IsLive) return;
            var config = _kit.Config;
            var contact = collision.GetContact(0);
            Vector2 away = _body.position - other._body.position;
            var normal = Vector2.Dot(contact.normal, away) >= 0f ? contact.normal : -contact.normal;
            var own = Vector2.Dot(_driver.PreSolveVelocity, normal);
            var theirs = Vector2.Dot(other._driver.PreSolveVelocity, normal);
            var approach = CarBounceRule.ApproachSpeed(own, theirs);
            if (!CarBounceRule.IsImpact(approach, config.MinImpactSpeed)) return;
            var final = CarBounceRule.FinalNormalSpeed(own, theirs, EffectiveMass, other.EffectiveMass, config.Bounciness, config.MinImpactSpeed);
            _body.linearVelocity += normal * (final - Vector2.Dot(_body.linearVelocity, normal));
            if (_driver.Id.Value < other._driver.Id.Value) Report(contact.point, approach, other._driver);
        }

        // A car with nobody in it is braked: it counts heavier, so a driven car bounces off it and it only shuffles.
        private float EffectiveMass => _body.mass * (_driver.HasDriver ? 1f : _kit.Config.ParkedMassScale);

        private void Report(Vector2 point, float approach, CarDriver other)
        {
            if (!Services.TryGet<IEventBus>(out var bus) || !_kit.Limiter.TryPass(_driver.Id, other.Id, Time.time)) return;
            var strength = CarBounceRule.Strength01(approach, _kit.Config.MinImpactSpeed, _kit.Config.FullImpactSpeed);
            bus.Publish(new CarsCollided(point.x, point.y, strength));
        }
    }
}
