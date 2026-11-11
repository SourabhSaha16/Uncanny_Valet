using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Tests.Director
{
    /// <summary>One car in the soak test's fake lot.</summary>
    internal sealed class SoakCar
    {
        public CarId Id;
        public bool Dirty;
        public bool Ev;
        public bool WashDone;
        public bool ChargeDone;
        public CarLotState State;
        /// <summary>Regular spot id 1..9, or 0 when not in a regular spot.</summary>
        public int Spot;
        public bool InWash;
        /// <summary>Index of the EV bay it sits in, or -1.</summary>
        public int Bay = -1;
        public float ArriveLeft;
        public float StayLeft;
        public float WashLeft;
        public float ChargeLeft;
        public float PickupWaited;
        /// <summary>The stay as first planned, and what the director moved it by (negative = earlier).</summary>
        public float PlannedStay;
        public float Shifted;
        /// <summary>Shift seconds at which it arrived (the stay clock starts there).</summary>
        public float ParkedAt;
        /// <summary>Order in which the pickup was requested (lower is older).</summary>
        public int RequestOrder;

        public bool NeedsWash => Dirty && !WashDone;
        public bool NeedsCharge => Ev && !ChargeDone;

        public bool AwaitingPark => State == CarLotState.Arriving || State == CarLotState.WaitingToPark
            || State == CarLotState.Driven;

        public bool OpenTask => AwaitingPark || State == CarLotState.PickupRequested;
    }
}
