using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure steering maths of docs/redesign/car_handling_spec.md sections 2 and 3.</summary>
    public static class SteeringModel
    {
        /// <summary>
        /// Counter-clockwise turn rate in radians per second: MaxTurnRate * steer * clamp01(|forward| / TurnFullSpeed).
        /// Positive steer is right (clockwise). Zero at rest; the sign flips in reverse like a real car.
        /// </summary>
        public static float TurnRate(float steer, float forwardSpeed, float maxTurnRate, float turnFullSpeed)
        {
            var scale = Mathf.Clamp(forwardSpeed / Mathf.Max(0.01f, turnFullSpeed), -1f, 1f);
            return -Mathf.Clamp(steer, -1f, 1f) * maxTurnRate * scale;
        }

        /// <summary>Radius of the path at a forward speed (unit): constant R_min up to TurnFullSpeed, then speed / MaxTurnRate.</summary>
        public static float Radius(float forwardSpeed, float rMin, float maxTurnRate)
            => TurningRadius.RadiusAtSpeed(forwardSpeed, rMin, maxTurnRate);
    }
}
