using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
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
        private readonly Collider2D[] _overlaps = new Collider2D[8];
        private readonly float _radius;
        private ContactFilter2D _filter = new ContactFilter2D { useTriggers = false };
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
            _radius = config.ColliderRadius;
            _sensor = new WalkerSensor(_body);
        }

        /// <summary>
        /// Turns the collider on or off (off while passing through a blocker or when standing). Switching on re-applies the own-car
        /// exemption and never happens while the person stands inside a car: it stays off until they are clear (<see cref="WalkerSolidRule"/>).
        /// </summary>
        public void SetSolid(bool solid)
        {
            if (_collider.enabled == solid) return;
            if (solid && !WalkerSolidRule.Resolve(true, TouchesCar())) return;
            _collider.enabled = solid;
            if (solid) ApplyIgnore(true);
        }

        /// <summary>True when something solid other than <paramref name="ownCar"/> lies between <paramref name="from"/> and <paramref name="point"/> (a look-ahead that follows route corners).</summary>
        public bool IsBlockedToward(Vector2 from, Vector2 point, CarId ownCar)
        {
            var ahead = point - from;
            var length = ahead.magnitude;
            return length > Mathf.Epsilon && _sensor.IsBlocked(ahead / length, length, ownCar);
        }

        // Any car other than the own one overlapping the body's circle right now (the collider is off when this runs).
        private bool TouchesCar()
        {
            var count = Physics2D.OverlapCircle(_body.position, _radius, _filter, _overlaps);
            for (var i = 0; i < count; i++)
            {
                if (_overlaps[i].GetComponentInParent<IDrivable>() == null) continue;
                if (_ownCar == null || !_overlaps[i].transform.IsChildOf(_ownCar)) return true;
            }
            return false;
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

        private void ApplyIgnore(bool ignore)
        {
            if (_ownCar == null) return;
            _ownCar.GetComponentsInChildren(_scratch);
            foreach (var c in _scratch) Physics2D.IgnoreCollision(_collider, c, ignore);
            _scratch.Clear();
        }
    }
}
