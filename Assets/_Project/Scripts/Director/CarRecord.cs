using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Director
{
    /// <summary>Everything the tracker knows about one open ticket's car. Written only by <see cref="CarBook"/>.</summary>
    internal sealed class CarRecord
    {
        public const int NoSpot = 0;

        public CarId Car;
        public bool IsEv;
        public bool IsDirty;
        public bool Arrived;
        public bool Driven;
        public bool AwaitingPark = true;
        public bool PickupRequested;
        public bool InStation;
        public StationKind Station;
        public int SpotId = NoSpot;
        public bool WashDone;
        public bool ChargeDone;
        public float Clean01;
        public float PickupElapsed01;
        public float StayMinutes;
        /// <summary>Game minutes the director has moved the return by (negative = earlier).</summary>
        public float ShiftedMinutes;
        public float ParkedAtMinute = float.NaN;

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

        public bool NeedsWash => IsDirty && !WashDone;
        public bool NeedsCharge => IsEv && !ChargeDone;

        /// <summary>Patience left on the pickup timer; 1 until a pickup is requested (no park window).</summary>
        public float Patience01 => PickupRequested ? 1f - PickupElapsed01 : 1f;
    }
}
