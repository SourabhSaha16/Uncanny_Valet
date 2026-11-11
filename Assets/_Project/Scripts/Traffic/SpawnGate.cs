namespace UncannyValet.Traffic
{
    /// <summary>Pure rule: may an ambient car appear at a place on a lane? Only into a free gap, the same gap a merging car needs.</summary>
    public static class SpawnGate
    {
        /// <summary>True when a car of <paramref name="length"/> centred at <paramref name="distance"/> on <paramref name="lane"/> overlaps nobody and has room ahead and behind.</summary>
        public static bool IsOpen(LaneOccupancy occupancy, MergeGapRule merge, TrafficLane lane, float distance, float length)
        {
            if (occupancy.Overlaps(lane, distance, length)) return false;
            float ahead = occupancy.GapAhead(lane, distance, length, 0, out _);
            float behind = occupancy.GapBehind(lane, distance, length, 0, out float followerSpeed);
            return merge.CanMerge(ahead, behind, followerSpeed);
        }
    }
}
