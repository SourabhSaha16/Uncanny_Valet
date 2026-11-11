using System.Collections;
using UncannyValet.Cars;
using UnityEngine;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>
    /// Drives a car along the section 3 bay-entry manoeuvre: hold a cruise speed, full lock until the nose points down
    /// the bay, straight until the stopping distance, then brake. Written against <see cref="CarDriver.SetInput"/>
    /// only, so a new steering model (rollout step 2) needs no change here.
    /// </summary>
    public static class BayEntryDriver
    {
        public const float CruiseSpeed = 2f;
        private const float AlignedDegrees = 2f;
        private const float StoppedSpeed = 0.05f;

        /// <summary>Runs the manoeuvre; ends when the car has stopped or <paramref name="timeout"/> seconds have passed.</summary>
        /// <param name="turnLeft">True when the bay lies left of the heading (counter-clockwise arc).</param>
        /// <param name="bayAxisDegrees">Heading the car must end with (0 = up).</param>
        /// <param name="bayCentre">Point the car's centre should stop on.</param>
        public static IEnumerator Run(CarDriver car, Rigidbody2D body, Vector2 bayCentre, float bayAxisDegrees, bool turnLeft, float timeout)
        {
            var steer = turnLeft ? -1f : 1f;
            Vector2 axis = Quaternion.Euler(0f, 0f, bayAxisDegrees) * Vector2.up;
            var aligned = false;
            var elapsed = 0f;
            while (elapsed < timeout)
            {
                yield return new WaitForFixedUpdate();
                elapsed += Time.fixedDeltaTime;
                var speed = body.linearVelocity.magnitude;
                if (!aligned && Mathf.Abs(Mathf.DeltaAngle(body.rotation, bayAxisDegrees)) < AlignedDegrees) aligned = true;
                var remaining = Vector2.Dot(bayCentre - body.position, axis);
                var stopping = speed * speed / (2f * car.Definition.Braking);
                var braking = aligned && remaining <= stopping;
                var throttle = braking ? (speed > StoppedSpeed ? -1f : 0f) : (speed < CruiseSpeed ? 1f : 0f);
                car.SetInput(throttle, aligned ? 0f : steer);
                if (braking && speed <= StoppedSpeed) yield break;
            }
        }
    }
}
