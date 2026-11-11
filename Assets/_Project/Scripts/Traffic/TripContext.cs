using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>What every patron trip shares: the road, the east lane path, the occupancy registry and the rules.</summary>
    public sealed class TripContext
    {
        private const int ScratchCapacity = 16;

        public readonly IRoadLayout Road;
        public readonly LanePath EastPath;
        public readonly LaneOccupancy Occupancy;
        public readonly FollowSettings Follow;
        public readonly MergeGapRule Merge;
        public readonly TripTuning Tuning;
        /// <summary>Live entry and exit speed scales.</summary>
        public readonly TripSpeeds Speeds;
        /// <summary>Reusable buffer for fetching routes from the road layout.</summary>
        public readonly List<Vector2> Scratch = new List<Vector2>(ScratchCapacity);

        public TripContext(IRoadLayout road, LanePath eastPath, LaneOccupancy occupancy,
            FollowSettings follow, MergeGapRule merge, TripTuning tuning, TripSpeeds speeds = null)
        {
            Road = road;
            EastPath = eastPath;
            Occupancy = occupancy;
            Follow = follow;
            Merge = merge;
            Tuning = tuning;
            Speeds = speeds ?? new TripSpeeds(1f, 1f);
        }

        /// <summary>Who decides the queue slot of an arriving car; null until Patrons registers it (see <see cref="TryGetSlots"/>).</summary>
        public IQueueSlotSource Slots { get; set; }

        /// <summary>Finds the slot source, looking in Services until it is there.</summary>
        public bool TryGetSlots(out IQueueSlotSource slots)
        {
            if (Slots == null && Services.TryGet<IQueueSlotSource>(out var found)) Slots = found;
            slots = Slots;
            return slots != null;
        }

        /// <summary>True when a car centred at <paramref name="distance"/> on the east lane has room to join it now.</summary>
        public bool CanJoinEast(float distance)
        {
            float length = Tuning.CarLength;
            float ahead = Occupancy.GapAhead(TrafficLane.East, distance, length, 0, out _);
            float behind = Occupancy.GapBehind(TrafficLane.East, distance, length, 0, out float followerSpeed);
            return Merge.CanMerge(ahead, behind, followerSpeed);
        }

        /// <summary>
        /// Keeps a car that has turned off the east lane registered on it (at the distance nearest its position, at its
        /// speed) until it is <see cref="TripTuning.LaneClearance"/> away from the lane line, so lane traffic still brakes for it.
        /// Does nothing when the car has no registration.
        /// </summary>
        public void TrackLane(ref LaneState state, Vector2 position, float speed)
        {
            if (state.Id == 0) return;
            float distance = EastPath.Project(position, out float lateral);
            if (lateral >= Tuning.LaneClearance)
            {
                Occupancy.Remove(state.Id);
                state = default;
                return;
            }
            state.Distance = distance;
            state.Speed = speed;
            Occupancy.Update(state.Id, distance, speed);
        }

        /// <summary>Registers a car on the east lane and returns its lane state.</summary>
        public LaneState JoinEast(float distance, float speed)
        {
            int id = Occupancy.Add(TrafficLane.East, distance, speed, Tuning.CarLength);
            return new LaneState { Id = id, Lane = TrafficLane.East, Distance = distance, Speed = speed };
        }
    }
}
