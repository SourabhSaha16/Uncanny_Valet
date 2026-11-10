using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>Moves one car along its lane with car following, and keeps the occupancy registry in step.</summary>
    public static class LaneMotion
    {
        /// <summary>
        /// Advances <paramref name="state"/> by <paramref name="dt"/>. The car wants <paramref name="desiredSpeed"/>
        /// (never above <paramref name="speedCap"/>) and slows for the car ahead.
        /// </summary>
        public static void Step(ref LaneState state, float carLength, float desiredSpeed, float speedCap,
            LaneOccupancy occupancy, in FollowSettings follow, float dt)
        {
            float gap = occupancy.GapAhead(state.Lane, state.Distance, carLength, state.Id, out float leaderSpeed);
            FollowSettings settings = follow.WithDesiredSpeed(Mathf.Min(desiredSpeed, speedCap));
            state.Speed = CarFollowing.NextSpeed(state.Speed, gap, leaderSpeed, settings, dt);
            state.Distance += state.Speed * dt;
            occupancy.Update(state.Id, state.Distance, state.Speed);
        }
    }
}
