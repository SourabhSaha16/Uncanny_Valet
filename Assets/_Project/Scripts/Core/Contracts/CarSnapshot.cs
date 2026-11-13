using System;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Core.Contracts
{
    /// <summary>One open ticket's car as the director sees it. Plain data.</summary>
    public readonly struct CarSnapshot
    {
        /// <summary>Number of station kinds (the length of the per-station arrays).</summary>
        public static readonly int StationKindCount = Enum.GetValues(typeof(StationKind)).Length;

        private readonly float[] _leftAtStation;

        public readonly CarId Car;
        public readonly CarLotState State;
        /// <summary>The modifiers the car arrived with (its modifier list).</summary>
        public readonly ModifierSet Held;
        /// <summary>The modifiers already cleared.</summary>
        public readonly ModifierSet Done;
        /// <summary>The ticket is still waiting for its first parking (arriving, waiting or being driven there).</summary>
        public readonly bool AwaitingPark;
        /// <summary>Patience of the running park or pickup timer: 1 full, 0 out; 1 when no timer is running.</summary>
        public readonly float Patience01;
        /// <summary>Seconds until the patron wants the car (parked cars only); positive infinity otherwise.</summary>
        public readonly float SecondsToPickup;
        /// <summary>The patron's planned stay in seconds, before the director moved it (0 when unknown).</summary>
        public readonly float PlannedStaySeconds;
        /// <summary>Seconds the director has already moved the return by; negative means earlier.</summary>
        public readonly float ReturnShiftSeconds;

        /// <summary>The modifiers still open: held and not cleared.</summary>
        public ModifierSet Open => Held.Without(Done);

        /// <summary>True while at least one modifier is open (the car needs a service).</summary>
        public bool NeedsService => !Open.IsEmpty;

        /// <summary>True while an open modifier is cleared at this kind of station.</summary>
        public bool NeedsAt(StationKind kind) => LeftAt(kind) > 0f;

        /// <summary>
        /// Service still to do at this kind of station, in units of one full service: 0 when nothing is open there, up to 1
        /// per open modifier (a half-washed Muddy car reads 0.5).
        /// </summary>
        public float LeftAt(StationKind kind)
        {
            var i = (int)kind;
            return _leftAtStation != null && i >= 0 && i < _leftAtStation.Length ? _leftAtStation[i] : 0f;
        }

        /// <param name="leftAtStation">Per station kind (indexed by StationKind) the service still to do there; null means none. Not copied.</param>
        public CarSnapshot(CarId car, CarLotState state, ModifierSet held, ModifierSet done, bool awaitingPark,
            float patience01, float secondsToPickup, float plannedStaySeconds = 0f, float returnShiftSeconds = 0f,
            float[] leftAtStation = null)
        {
            PlannedStaySeconds = plannedStaySeconds; ReturnShiftSeconds = returnShiftSeconds;
            Car = car; State = state; Held = held; Done = done;
            AwaitingPark = awaitingPark; Patience01 = patience01; SecondsToPickup = secondsToPickup;
            _leftAtStation = leftAtStation;
        }
    }
}
