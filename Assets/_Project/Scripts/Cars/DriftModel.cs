using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Pure vectored-inertia maths of docs/redesign/car_handling_spec.md section 2. The velocity lags behind the facing;
    /// the gap is the slip angle. All speeds are world units per second.
    /// </summary>
    public static class DriftModel
    {
        /// <summary>
        /// Grip for this step: 1 up to GripLowSpeed, blending to GripAtSpeed at GripHighSpeed, scaled while the handbrake is held.
        /// <paramref name="gripLossScale"/> scales the loss (1 minus grip): 1 changes nothing, 0.4 keeps 40% of the loss.
        /// </summary>
        public static float Grip(float speed, bool drift, in DriftSettings s, float gripLossScale = 1f)
        {
            var blend = Mathf.InverseLerp(s.GripLowSpeed, s.GripHighSpeed, speed);
            var grip = Mathf.Lerp(1f, s.GripAtSpeed, blend);
            var normal = drift && speed > s.DriftMinSpeed ? grip * s.DriftGripScale : grip;
            return gripLossScale == 1f ? normal : 1f - (1f - normal) * gripLossScale;
        }

        /// <summary>
        /// One step: damps the sideways part of <paramref name="velocity"/> (relative to the unit <paramref name="facing"/>)
        /// and hands part of the speed that removal costs back along the direction of travel.
        /// </summary>
        public static Vector2 Step(Vector2 velocity, Vector2 facing, bool drift, in DriftSettings s, float dt, float gripLossScale = 1f)
        {
            var right = new Vector2(facing.y, -facing.x);
            var forward = Vector2.Dot(velocity, facing);
            var sideways = Vector2.Dot(velocity, right);
            var grip = Grip(velocity.magnitude, drift, s, gripLossScale);
            var kept = sideways * Mathf.Exp(-s.LateralFriction * grip * dt);
            // Damping the sideways part lowers the speed (hypot); ForwardGrip of that loss is handed back along the
            // travel direction. 1 keeps the speed exactly (a slide turns into a curve), 0 scrubs it.
            var before = Mathf.Sqrt(forward * forward + sideways * sideways);
            var damped = Mathf.Sqrt(forward * forward + kept * kept);
            var target = damped + s.ForwardGrip * (before - damped);
            var handed = Mathf.Sqrt(Mathf.Max(0f, target * target - kept * kept));
            var direction = forward > 0f ? 1f : forward < 0f ? -1f : 0f;
            return facing * (handed * direction) + right * kept;
        }

        /// <summary>Angle in degrees between the facing and the direction of travel, ignoring left/right and forward/back: atan2(|sideways|, |forward|).</summary>
        public static float SlipAngleDegrees(Vector2 velocity, Vector2 facing)
        {
            var forward = Vector2.Dot(velocity, facing);
            var sideways = Vector2.Dot(velocity, new Vector2(facing.y, -facing.x));
            return SlipAngleDegrees(forward, sideways);
        }

        /// <summary>The same angle from the forward and sideways speeds already split.</summary>
        public static float SlipAngleDegrees(float forward, float sideways)
            => Mathf.Atan2(Mathf.Abs(sideways), Mathf.Abs(forward)) * Mathf.Rad2Deg;
    }
}
