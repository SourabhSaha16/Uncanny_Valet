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

        /// <summary>
        /// Share of the turn rate kept at a speed: 1 up to <paramref name="turnFullSpeed"/>, then it eases down by
        /// <paramref name="falloffPerSpeed"/> per unit of speed, never below <paramref name="minShare"/>.
        /// </summary>
        public static float Falloff(float speed, float turnFullSpeed, float falloffPerSpeed, float minShare)
        {
            var over = Mathf.Max(0f, Mathf.Abs(speed) - turnFullSpeed);
            return Mathf.Clamp(1f - falloffPerSpeed * over, Mathf.Clamp01(minShare), 1f);
        }

        /// <summary>Turn rate with the fast-driving fall-off applied (see <see cref="Falloff"/>).</summary>
        public static float TurnRate(float steer, float forwardSpeed, float maxTurnRate, float turnFullSpeed,
            float falloffPerSpeed, float minShare)
            => TurnRate(steer, forwardSpeed, maxTurnRate, turnFullSpeed) * Falloff(forwardSpeed, turnFullSpeed, falloffPerSpeed, minShare);

        /// <summary>Radius with the fall-off: speed divided by the eased turn rate, never below R_min.</summary>
        public static float Radius(float forwardSpeed, float rMin, float maxTurnRate, float turnFullSpeed,
            float falloffPerSpeed, float minShare)
            => TurningRadius.RadiusAtSpeed(forwardSpeed, rMin,
                maxTurnRate * Falloff(forwardSpeed, turnFullSpeed, falloffPerSpeed, minShare));

        /// <summary>Radius of the path at a forward speed (unit): constant R_min up to TurnFullSpeed, then speed / MaxTurnRate.</summary>
        public static float Radius(float forwardSpeed, float rMin, float maxTurnRate)
            => TurningRadius.RadiusAtSpeed(forwardSpeed, rMin, maxTurnRate);
    }
}
