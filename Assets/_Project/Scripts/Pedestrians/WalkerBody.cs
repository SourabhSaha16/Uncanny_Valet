using UncannyValet.Core;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>The physics side of one person: a kinematic body with a circle collider on the pedestrian layer, plus the look-ahead sensor.</summary>
    public sealed class WalkerBody
    {
        private readonly Rigidbody2D _body;
        private readonly CircleCollider2D _collider;
        private readonly WalkerSensor _sensor;

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

        /// <summary>Turns the collider on or off (off while passing through a blocker or when standing).</summary>
        public void SetSolid(bool solid) => _collider.enabled = solid;

        /// <summary>Jumps body and transform to a point.</summary>
        public void Teleport(Vector2 position)
        {
            _body.transform.position = position;
            _body.position = position;
        }

        /// <summary>Moves the body to a point during the physics step.</summary>
        public void MoveTo(Vector2 position) => _body.MovePosition(position);

        /// <summary>True when something solid lies within <paramref name="distance"/> along <paramref name="heading"/>.</summary>
        public bool IsBlocked(Vector2 heading, float distance) => _sensor.IsBlocked(heading, distance);
    }
}
