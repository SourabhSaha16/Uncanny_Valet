using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    public sealed class FakeClock : IGameClock
    {
        public float SecondsRemaining { get; set; } = 240f;
        public float ShiftLengthSeconds { get; set; } = 240f;
        public float ElapsedMinutes { get; set; }
        public bool IsRunning { get; set; } = true;

        /// <summary>Moves the shift to this many elapsed seconds (1 game minute per second).</summary>
        public void SetElapsed(float seconds)
        {
            SecondsRemaining = ShiftLengthSeconds - seconds;
            ElapsedMinutes = seconds;
        }
    }

    /// <summary>Always answers the middle of a range, so tests do not depend on luck.</summary>
    internal sealed class MidRandom : IDirectorRandom
    {
        public float Value() => 0.5f;
        public float Range(float min, float max) => (min + max) * 0.5f;
    }

    /// <summary>A small seeded generator, so the same seed gives the same draws.</summary>
    internal sealed class TestRandom : IDirectorRandom
    {
        private readonly System.Random _inner;
        public TestRandom(int seed) { _inner = new System.Random(seed); }
        public float Value() => (float)_inner.NextDouble();
        public float Range(float min, float max) => min + (max - min) * Value();
    }

    internal sealed class FakeLot : ILotSnapshotSource
    {
        private readonly FakeClock _clock;
        public LotSnapshot Next;
        public FakeLot(FakeClock clock) { _clock = clock; Next = LotSnapshot.Empty; }

        public LotSnapshot Capture()
        {
            var s = Next;
            return new LotSnapshot(s.Cars, s.Slots, s.FreeSpots, s.TotalSpots, s.SecondsToNextPickup,
                _clock.ShiftLengthSeconds - _clock.SecondsRemaining, s.AngryTimes);
        }
    }

    /// <summary>Builds snapshots for tests with a short, readable syntax.</summary>
    internal sealed class SnapshotBuilderForTests
    {
        private readonly List<CarSnapshot> _cars = new List<CarSnapshot>();
        private readonly List<StationSlot> _slots = new List<StationSlot>();
        private int _next = 1;

        public int FreeSpots = 5;
        public int TotalSpots = 10;
        public float NowSeconds;
        public float NextPickup = float.PositiveInfinity;
        public readonly List<float> Angry = new List<float>();

        public CarId Car(CarLotState state, bool dirty = false, bool ev = false, bool washDone = false,
            bool chargeDone = false, bool awaitingPark = false, float washLeft = 0f, float patience = 1f,
            float toPickup = float.PositiveInfinity)
        {
            var id = new CarId(_next++);
            _cars.Add(new CarSnapshot(id, state, dirty, ev, washDone, chargeDone, awaitingPark, washLeft, patience, toPickup));
            return id;
        }

        public void Slot(StationKind kind, StationSlotState state, CarId car = default)
            => _slots.Add(new StationSlot(kind, state, car));

        public LotSnapshot Build() => new LotSnapshot(_cars, _slots, FreeSpots, TotalSpots,
            NextPickup, NowSeconds, Angry);
    }
}
