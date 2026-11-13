using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Director
{
    /// <summary>Everything the tracker knows about one open ticket's car. Written only by <see cref="CarBook"/>.</summary>
    internal sealed class CarRecord
    {
        public const int NoSpot = 0;

        public CarId Car;
        /// <summary>The service modifiers the car arrived with.</summary>
        public ModifierSet Held;
        /// <summary>The modifiers already cleared.</summary>
        public ModifierSet Done;
        /// <summary>Progress 0..1 per modifier id; written by <see cref="CarBook"/>.</summary>
        public readonly float[] Progress = new float[ModifierSet.Capacity];
        /// <summary>Per station kind: the service still to do there (sum over the open modifiers of one minus progress); 0 when none is open.</summary>
        public readonly float[] LeftAtStation = new float[CarSnapshot.StationKindCount];
        /// <summary>Bit per station kind that clears at least one modifier the car holds (open or done).</summary>
        public int HeldStations;
        public bool Arrived;
        public bool Driven;
        public bool AwaitingPark = true;
        public bool PickupRequested;
        public bool InStation;
        public StationKind Station;
        public int SpotId = NoSpot;
        public float PickupElapsed01;
        public float StayMinutes;
        /// <summary>Game minutes the director has moved the return by (negative = earlier).</summary>
        public float ShiftedMinutes;
        /// <summary>Game minute the patron's stay started (NaN until it did; see the StayStarted event).</summary>
        public float StayStartedAtMinute = float.NaN;

        /// <summary>The single state the car is in; earlier rules win (a requested pickup beats everything).</summary>
        public CarLotState State
        {
            get
            {
                if (!Arrived) return CarLotState.Arriving;
                if (PickupRequested) return CarLotState.PickupRequested;
                if (Driven) return CarLotState.Driven;
                if (InStation) return CarLotState.InStation;
                if (SpotId != NoSpot || !AwaitingPark) return CarLotState.Parked;
                return CarLotState.WaitingToPark;
            }
        }

        /// <summary>True while an open modifier is cleared at this kind of station.</summary>
        public bool NeedsAt(StationKind kind) => LeftAtStation[(int)kind] > 0f;

        /// <summary>True when a modifier of the car is cleared at this kind of station, cleared or not.</summary>
        public bool HoldsFor(StationKind kind) => (HeldStations & (1 << (int)kind)) != 0;

        /// <summary>Patience left on the pickup timer; 1 until a pickup is requested (no park window).</summary>
        public float Patience01 => PickupRequested ? 1f - PickupElapsed01 : 1f;
    }
}
