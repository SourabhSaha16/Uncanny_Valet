using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// Pure car-following rule: the speed a car may drive given the gap to its leader. It accelerates on a free road,
    /// brakes smoothly as the gap closes, and is hard-capped so it can always stop behind the leader.
    /// </summary>
    public static class CarFollowing
    {
        private const float MinimumStep = 0.0001f;

        /// <summary>The speed the car should aim for. <paramref name="gap"/> may be infinity (no leader).</summary>
        public static float TargetSpeed(float gap, float leaderSpeed, in FollowSettings settings)
        {
            if (float.IsPositiveInfinity(gap)) return settings.DesiredSpeed;
            float free = Mathf.Max(0f, gap - settings.MinGap);
            float byHeadway = free / settings.Headway;
            return Mathf.Min(settings.DesiredSpeed, byHeadway, StoppableSpeed(free, leaderSpeed, settings.Brake));
        }

        /// <summary>The next speed: moves toward the target at the acceleration or brake rate, never above the no-collision cap.</summary>
        public static float NextSpeed(float current, float gap, float leaderSpeed, in FollowSettings settings, float dt)
        {
            if (dt < MinimumStep) return current;
            float target = TargetSpeed(gap, leaderSpeed, settings);
            float next = Approach(current, target, settings.Acceleration, settings.Brake, dt);
            if (float.IsPositiveInfinity(gap)) return next;
            float free = Mathf.Max(0f, gap - settings.MinGap);
            float hardCap = StoppableSpeed(free, leaderSpeed, settings.Brake);
            return Mathf.Min(next, hardCap, Mathf.Max(0f, gap) / dt);
        }

        /// <summary>Moves <paramref name="current"/> toward <paramref name="target"/> by the acceleration or brake rate.</summary>
        public static float Approach(float current, float target, float acceleration, float brake, float dt)
        {
            float rate = target > current ? acceleration : brake;
            return Mathf.MoveTowards(current, target, rate * dt);
        }

        private static float StoppableSpeed(float free, float leaderSpeed, float brake) =>
            Mathf.Sqrt(leaderSpeed * leaderSpeed + 2f * brake * free);
    }
}
