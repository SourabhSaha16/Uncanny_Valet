using System.Collections.Generic;
using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// A read-only view of the lot at one moment: open tickets' cars, station slots, free spots. The lists are
    /// buffers owned by the tracker and stay valid until its next capture; copy what must outlive that.
    /// </summary>
    public readonly struct LotSnapshot
    {
        private static readonly IReadOnlyList<CarSnapshot> NoCars = new CarSnapshot[0];
        private static readonly IReadOnlyList<StationSlot> NoSlots = new StationSlot[0];
        private static readonly IReadOnlyList<float> NoTimes = new float[0];

        public readonly IReadOnlyList<CarSnapshot> Cars;
        public readonly IReadOnlyList<StationSlot> Slots;
        public readonly int FreeSpots;
        public readonly int TotalSpots;
        /// <summary>Seconds until the next parked car's patron wants it; positive infinity when none is parked.</summary>
        public readonly float SecondsToNextPickup;
        /// <summary>Shift seconds elapsed when the snapshot was taken.</summary>
        public readonly float NowSeconds;
        /// <summary>Shift seconds at which each recent angry patron stormed off.</summary>
        public readonly IReadOnlyList<float> AngryTimes;

        public LotSnapshot(IReadOnlyList<CarSnapshot> cars, IReadOnlyList<StationSlot> slots, int freeSpots,
            int totalSpots, float secondsToNextPickup, float nowSeconds, IReadOnlyList<float> angryTimes)
        {
            Cars = cars ?? NoCars; Slots = slots ?? NoSlots; FreeSpots = freeSpots; TotalSpots = totalSpots;
            SecondsToNextPickup = secondsToNextPickup; NowSeconds = nowSeconds; AngryTimes = angryTimes ?? NoTimes;
        }

        /// <summary>An empty lot.</summary>
        public static LotSnapshot Empty => new LotSnapshot(null, null, 0, 0, float.PositiveInfinity, 0f, null);

        /// <summary>How many cars are in the given state.</summary>
        public int CountCars(CarLotState state)
        {
            var n = 0;
            for (var i = 0; i < Cars.Count; i++) if (Cars[i].State == state) n++;
            return n;
        }

        /// <summary>How many slots of a station kind are in the given state.</summary>
        public int CountSlots(StationKind kind, StationSlotState state)
        {
            var n = 0;
            for (var i = 0; i < Slots.Count; i++) if (Slots[i].Kind == kind && Slots[i].State == state) n++;
            return n;
        }

        /// <summary>Angry patrons within the last <paramref name="windowSeconds"/> seconds.</summary>
        public int CountAngry(float windowSeconds)
        {
            var n = 0;
            for (var i = 0; i < AngryTimes.Count; i++) if (NowSeconds - AngryTimes[i] <= windowSeconds) n++;
            return n;
        }
    }
}
