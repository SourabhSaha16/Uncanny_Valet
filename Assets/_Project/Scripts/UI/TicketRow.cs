using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.UI
{
    /// <summary>Plain data for one open ticket. Written by TicketBoard, read by the painters.</summary>
    public sealed class TicketRow
    {
        /// <summary>Tag index used until the car's key-tag colour is known.</summary>
        public const int NoTag = -1;

        public int Car;
        /// <summary>The key-tag colour index (see TagPalette) of this ticket.</summary>
        public int Tag = NoTag;
        /// <summary>The car's licence plate text (for example "KT 4"); empty until it is known.</summary>
        public string Plate = string.Empty;
        /// <summary>The car's damage 0..1 (set by CarDamaged); the key board shows it only when above 0.</summary>
        public float Damage01;
        public TicketRowState State = TicketRowState.AwaitingPark;
        public int Spot;
        public bool HasTimer;
        public TimerKind Timer;
        /// <summary>0 = just started, 1 = out of time.</summary>
        public float Elapsed01;
        public bool Expired;

        /// <summary>True once the car was parked in a spot or a station; the key board lists a car from then on until it is delivered.</summary>
        public bool Placed;

        /// <summary>True while the car sits in a station (the wash slot or a charger); <see cref="Station"/> says which.</summary>
        public bool HasStation;
        public StationKind Station;

        /// <summary>True from the moment the car has finished driving in until it leaves.</summary>
        public bool Arrived;

        /// <summary>The car's service modifiers (a list: a car may hold several), each with its own progress.</summary>
        public readonly List<RowModifier> Modifiers = new List<RowModifier>();

        /// <summary>The licence plate under the car is drawn only once the plate is known and the car is parked in a spot or station (the CAR SLOTS rule).</summary>
        public bool ShowsPlate => HasPlate && Placed;

        /// <summary>True while the car is in the given kind of station.</summary>
        public bool InStation(StationKind kind) => HasStation && Station == kind;

        /// <summary>True once the plate text is known.</summary>
        public bool HasPlate => Plate.Length > 0;

        /// <summary>The modifier row of <paramref name="id"/>, or null.</summary>
        public RowModifier Find(ModifierId id)
        {
            for (var i = 0; i < Modifiers.Count; i++)
                if (Modifiers[i].Id == id) return Modifiers[i];
            return null;
        }

        /// <summary>The first open modifier cleared at this kind of station, or null.</summary>
        public RowModifier OpenAt(StationKind kind)
        {
            for (var i = 0; i < Modifiers.Count; i++)
                if (Modifiers[i].IsOpen && Modifiers[i].ClearedAt == kind) return Modifiers[i];
            return null;
        }

        /// <summary>The first modifier that just cleared and whose done word is still showing, or null.</summary>
        public RowModifier JustCleared()
        {
            for (var i = 0; i < Modifiers.Count; i++)
                if (Modifiers[i].ShowDone) return Modifiers[i];
            return null;
        }

        /// <summary>The car still needs a service: at least one modifier is open.</summary>
        public bool NeedsService
        {
            get
            {
                for (var i = 0; i < Modifiers.Count; i++)
                    if (Modifiers[i].IsOpen) return true;
                return false;
            }
        }

        /// <summary>The floating info box appears only after arrival and only while a modifier meter applies.</summary>
        public bool ShowsInfoBox => Arrived && NeedsService;

        public void StartTimer(TimerKind kind, float elapsed01)
        {
            HasTimer = true;
            Timer = kind;
            Elapsed01 = elapsed01;
        }
    }
}
