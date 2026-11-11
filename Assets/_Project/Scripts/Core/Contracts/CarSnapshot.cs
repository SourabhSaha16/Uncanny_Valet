using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>One open ticket's car as the director sees it. Plain data.</summary>
    public readonly struct CarSnapshot
    {
        public readonly CarId Car;
        public readonly CarLotState State;
        public readonly bool IsDirty;
        public readonly bool IsEv;
        public readonly bool WashDone;
        public readonly bool ChargeDone;
        /// <summary>The ticket is still waiting for its first parking (arriving, waiting or being driven there).</summary>
        public readonly bool AwaitingPark;
        /// <summary>Wash still to do, 0 (clean or not dirty) to 1 (filthy).</summary>
        public readonly float WashLeft01;
        /// <summary>Patience of the running park or pickup timer: 1 full, 0 out; 1 when no timer is running.</summary>
        public readonly float Patience01;
        /// <summary>Seconds until the patron wants the car (parked cars only); positive infinity otherwise.</summary>
        public readonly float SecondsToPickup;
        /// <summary>The patron's planned stay in seconds, before the director moved it (0 when unknown).</summary>
        public readonly float PlannedStaySeconds;
        /// <summary>Seconds the director has already moved the return by; negative means earlier.</summary>
        public readonly float ReturnShiftSeconds;

        public bool NeedsWash => IsDirty && !WashDone;
        public bool NeedsCharge => IsEv && !ChargeDone;

        public CarSnapshot(CarId car, CarLotState state, bool isDirty, bool isEv, bool washDone, bool chargeDone,
            bool awaitingPark, float washLeft01, float patience01, float secondsToPickup,
            float plannedStaySeconds = 0f, float returnShiftSeconds = 0f)
        {
            PlannedStaySeconds = plannedStaySeconds; ReturnShiftSeconds = returnShiftSeconds;
            Car = car; State = state; IsDirty = isDirty; IsEv = isEv; WashDone = washDone; ChargeDone = chargeDone;
            AwaitingPark = awaitingPark; WashLeft01 = washLeft01; Patience01 = patience01;
            SecondsToPickup = secondsToPickup;
        }
    }
}
