using System;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
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
        private enum Mode { Idle, Arriving, Departing, TripArriving, TripDeparting }

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
        private float _finalRotation;
        private readonly CarTrip _trip = new CarTrip();

        /// <summary>True while the car is driving in or out on its own, by road trip or straight.</summary>
        public bool IsBusy => _mode != Mode.Idle;
        /// <summary>True once the car has started leaving.</summary>
        public bool IsDeparting => _mode == Mode.Departing || _mode == Mode.TripDeparting;

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
            if (IsDeparting) return;
            Cache(definition);
            _trip.Cancel();
            _onLeft = onLeft;
            _heading = transform.up;
            _speed = _mode == Mode.Arriving ? _speed : 0f;
            _travelled = 0f;
            SetAutoDriving(true);
            _mode = Mode.Departing;
        }

        /// <summary>
        /// Starts an entry trip: the car is placed at the trip's first pose (off-screen), drives the road and the queue
        /// route, and stops at <paramref name="target"/> facing <paramref name="rotationDegrees"/>.
        /// </summary>
        public void BeginArrivingTrip(CarDefinition definition, ITripDriver trips, int tripId, Vector2 target, float rotationDegrees)
        {
            Cache(definition);
            _target = target;
            _finalRotation = rotationDegrees;
            _trip.Begin(trips, tripId);
            SetAutoDriving(true);
            _mode = Mode.TripArriving;
            if (_trip.Advance(0f, out TripPose first) == TripStatus.Running) Place(first);
        }

        /// <summary>
        /// Starts an exit trip when the car stands where the exit route begins; returns false (and changes nothing)
        /// otherwise or when no trip is free. <paramref name="onLeft"/> runs when the car has driven off the road.
        /// </summary>
        public bool TryBeginDepartingTrip(CarDefinition definition, IExitRouter exits, ITripDriver trips, Action onLeft)
        {
            Cache(definition);
            int id = exits.BeginExit();
            if (id == TripId.None) return false;
            if (!trips.Step(id, 0f, out TripPose start) || Vector2.Distance(_body.position, start.Position) > _style.TripStartRadius)
            {
                trips.Cancel(id);
                return false;
            }
            _trip.Begin(trips, id);
            _onLeft = onLeft;
            SetAutoDriving(true);
            _mode = Mode.TripDeparting;
            return true;
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
            else if (_mode != Mode.Idle) StepTrip(Time.fixedDeltaTime);
        }

        private void OnDestroy() => _trip.Cancel();

        private void StepTrip(float dt)
        {
            TripStatus status = _trip.Advance(dt, out TripPose pose);
            if (status == TripStatus.Running)
            {
                _body.MovePosition(pose.Position);
                _body.MoveRotation(CarAutoDriveMath.TurnToward(_body.rotation, pose.HeadingDegrees, _style.TripTurnRate, dt));
                return;
            }
            if (_mode == Mode.TripArriving) ArriveAtTarget();
            else
            {
                _mode = Mode.Idle;
                _onLeft?.Invoke();
            }
        }

        private void Place(TripPose pose)
        {
            _body.position = pose.Position;
            _body.rotation = pose.HeadingDegrees;
            transform.SetPositionAndRotation(pose.Position, Quaternion.Euler(0f, 0f, pose.HeadingDegrees));
        }

        private void ArriveAtTarget()
        {
            _body.position = _target;
            _body.rotation = _finalRotation;
            transform.SetPositionAndRotation(_target, Quaternion.Euler(0f, 0f, _finalRotation));
            _mode = Mode.Idle;
            SetAutoDriving(false);
            if (Services.TryGet<IEventBus>(out var bus)) bus.Publish(new CarArrived(_driver.Id));
        }

        private void StepArrive(float dt)
        {
            Vector2 position = _body.position;
            float remaining = Vector2.Distance(position, _target);
            float step = CarAutoDriveMath.ArriveStep(remaining, _style.ArriveSpeed, _style.ArriveEase, dt);
            _speed = dt > 0f ? step / dt : 0f;
            if (step >= remaining || CarAutoDriveMath.HasArrived(remaining - step))
            {
                ArriveAtTarget();
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
