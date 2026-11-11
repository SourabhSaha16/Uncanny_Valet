using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure throttle, brake and coast maths along the car's forward axis (grip and steering live in DriftModel and SteeringModel).</summary>
    public static class CarPhysicsMath
    {
        /// <summary>Next forward speed (negative is reverse) after one step.</summary>
        public static float NextForwardSpeed(float forward, float throttle, float acceleration,
            float braking, float coastDrag, float topSpeed, float reverseFactor, float dt)
        {
            throttle = Mathf.Clamp(throttle, -1f, 1f);
            float next;
            if (Mathf.Approximately(throttle, 0f))
                next = Mathf.MoveTowards(forward, 0f, coastDrag * dt);
            else if (forward * throttle < 0f)
                next = Mathf.MoveTowards(forward, 0f, braking * dt);
            else
                next = forward + throttle * acceleration * dt;
            return Mathf.Clamp(next, -topSpeed * reverseFactor, topSpeed);
        }
    }
}
