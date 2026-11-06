using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure steering, throttle and grip maths. Speeds are along the car's own axes.</summary>
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

        /// <summary>Counter-clockwise turn rate in degrees per second. Zero at rest, full at 30% of top speed.</summary>
        public static float TurnDegreesPerSecond(float steer, float forward, float turnRate, float topSpeed)
        {
            float reference = Mathf.Max(0.01f, topSpeed * 0.3f);
            float speedFactor = Mathf.Clamp(forward / reference, -1f, 1f);
            return -Mathf.Clamp(steer, -1f, 1f) * turnRate * speedFactor;
        }

        /// <summary>Sideways speed after grip damping; exponential so it is frame-rate independent.</summary>
        public static float DampSideways(float sideways, float grip, float dt)
        {
            return sideways * Mathf.Exp(-Mathf.Max(0f, grip) * dt);
        }
    }
}
