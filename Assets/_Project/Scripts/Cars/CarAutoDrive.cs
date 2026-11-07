using System;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Drives a car on its own: in to a stop point, or straight out of view. While active the body is kinematic,
    /// the collider is off (no shoving, no trigger events) and CarDriver and CarInteraction are disabled, so the
    /// valet cannot enter. Once arrived the car is restored and behaves like any spawned car.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CarDriver))]
    [DisallowMultipleComponent]
    public sealed class CarAutoDrive : MonoBehaviour
    {
        private enum Mode { Idle, Arriving, Departing }

        private Rigidbody2D _body;
        private CarDriver _driver;
        private Collider2D _collider;
        private CarInteraction _interaction;
        private CarAutoDriveStyle _style;
        private Action _onLeft;
        private Mode _mode;
        private Vector2 _target;
        private Vector2 _heading;
        private float _speed;
        private float _travelled;

        /// <summary>True while the car is driving in or out on its own.</summary>
        public bool IsBusy => _mode != Mode.Idle;
        /// <summary>True once the car has started leaving.</summary>
        public bool IsDeparting => _mode == Mode.Departing;

        /// <summary>Drives to <paramref name="target"/> along the current heading, easing to a stop.</summary>
        public void BeginArriving(CarDefinition definition, Vector2 target)
        {
            Cache(definition);
            _target = target;
            _heading = transform.up;
            _speed = 0f;
            SetAutoDriving(true);
            _mode = Mode.Arriving;
        }

        /// <summary>Accelerates along the heading until out of view, then calls <paramref name="onLeft"/>.</summary>
        public void BeginDeparting(CarDefinition definition, Action onLeft)
        {
            if (_mode == Mode.Departing) return;
            Cache(definition);
            _onLeft = onLeft;
            _heading = transform.up;
            _speed = _mode == Mode.Arriving ? _speed : 0f;
            _travelled = 0f;
            SetAutoDriving(true);
            _mode = Mode.Departing;
        }

        private void Cache(CarDefinition definition)
        {
            _style = definition.AutoDrive;
            if (_body == null) _body = GetComponent<Rigidbody2D>();
            if (_driver == null) _driver = GetComponent<CarDriver>();
            if (_collider == null) _collider = GetComponent<Collider2D>();
            if (_interaction == null) _interaction = GetComponent<CarInteraction>();
        }

        private void SetAutoDriving(bool on)
        {
            _body.bodyType = on ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _collider.enabled = !on;
            _driver.enabled = !on;
            _interaction.enabled = !on;
        }

        private void FixedUpdate()
        {
            if (_mode == Mode.Arriving) StepArrive(Time.fixedDeltaTime);
            else if (_mode == Mode.Departing) StepDepart(Time.fixedDeltaTime);
        }

        private void StepArrive(float dt)
        {
            Vector2 position = _body.position;
            float remaining = Vector2.Distance(position, _target);
            float step = CarAutoDriveMath.ArriveStep(remaining, _style.ArriveSpeed, _style.ArriveEase, dt);
            _speed = dt > 0f ? step / dt : 0f;
            if (step >= remaining || CarAutoDriveMath.HasArrived(remaining - step))
            {
                _body.position = _target;
                _mode = Mode.Idle;
                SetAutoDriving(false);
                return;
            }
            _body.MovePosition(position + _heading * step);
        }

        private void StepDepart(float dt)
        {
            _speed = CarAutoDriveMath.DepartSpeed(_speed, _style.ExitAcceleration, _style.ExitSpeed, dt);
            _travelled += _speed * dt;
            _body.MovePosition(_body.position + _heading * (_speed * dt));
            if (!CarAutoDriveMath.HasLeft(_travelled, _style.ExitDistance)) return;
            _mode = Mode.Idle;
            _onLeft?.Invoke();
        }
    }
}
