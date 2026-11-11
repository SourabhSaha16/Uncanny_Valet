using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>The physics side of one person: a kinematic body with a circle collider on the pedestrian layer, the look-ahead sensor, and the ignored own car.</summary>
    public sealed class WalkerBody
    {
        private readonly Rigidbody2D _body;
        private readonly CircleCollider2D _collider;
        private readonly WalkerSensor _sensor;
        private readonly List<Collider2D> _scratch = new List<Collider2D>(4);
        private Transform _ownCar;

        public WalkerBody(GameObject go, PedestrianConfig config)
        {
            go.layer = PhysicsLayers.Pedestrian;
            _body = go.GetComponent<Rigidbody2D>();
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.gravityScale = 0f;
            _body.freezeRotation = true;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _collider = go.GetComponent<CircleCollider2D>();
            _collider.radius = config.ColliderRadius / config.Scale;
            _sensor = new WalkerSensor(_body);
        }

        /// <summary>Turns the collider on or off (off while passing through a blocker or when standing). Switching on re-applies the own-car exemption.</summary>
        public void SetSolid(bool solid)
        {
            if (_collider.enabled == solid) return;
            _collider.enabled = solid;
            if (solid) ApplyIgnore(true);
        }

        /// <summary>Makes this person pass through <paramref name="car"/> (their own car) until <see cref="ReleaseCar"/>.</summary>
        public void IgnoreCar(Transform car)
        {
            ReleaseCar();
            _ownCar = car;
            ApplyIgnore(true);
        }

        /// <summary>Stops ignoring the own car (despawn or a new patron).</summary>
        public void ReleaseCar()
        {
            ApplyIgnore(false);
            _ownCar = null;
        }

        /// <summary>Re-applies the own-car exemption (a collider that was switched off or deactivated forgets it).</summary>
        public void ReapplyIgnore() => ApplyIgnore(true);

        /// <summary>Jumps body and transform to a point.</summary>
        public void Teleport(Vector2 position)
        {
            _body.transform.position = position;
            _body.position = position;
        }

        /// <summary>Moves the body to a point during the physics step.</summary>
        public void MoveTo(Vector2 position) => _body.MovePosition(position);

        /// <summary>True when something solid other than <paramref name="ownCar"/> lies within <paramref name="distance"/> along <paramref name="heading"/>.</summary>
        public bool IsBlocked(Vector2 heading, float distance, CarId ownCar) => _sensor.IsBlocked(heading, distance, ownCar);

        private void ApplyIgnore(bool ignore)
        {
            if (_ownCar == null) return;
            _ownCar.GetComponentsInChildren(_scratch);
            foreach (var c in _scratch) Physics2D.IgnoreCollision(_collider, c, ignore);
            _scratch.Clear();
        }
    }
}
