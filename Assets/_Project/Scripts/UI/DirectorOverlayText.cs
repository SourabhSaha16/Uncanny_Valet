using System.Text;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.UI
{
    /// <summary>Pure text and colour rules for the F3 director overlay. Called a few times a second, never per frame.</summary>
    public static class DirectorOverlayText
    {
        private const int SecondsPerMinute = 60;

        /// <summary>m:ss from shift seconds.</summary>
        public static string Clock(float seconds)
        {
            var total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return (total / SecondsPerMinute) + ":" + (total % SecondsPerMinute).ToString("00");
        }

        public static string Header(IDirectorView v)
            => (v.Active ? "Director ON" : "Director OFF (old timer)") + "   phase " + v.Phase
               + "   W " + v.Workload.ToString("0.00") + "  T " + v.Target.ToString("0.00")
               + "  P " + v.Pressure.ToString("0.00") + "  F " + v.Form.ToString("+0.00;-0.00;0.00");

        public static string Arrivals(IDirectorView v)
            => "Arrivals " + v.ArrivalsUsed + "/" + v.ArrivalBudget + " (cap " + v.ArrivalBudgetMax + ")   next in "
               + v.SecondsToNextArrival.ToString("0.0") + " s" + (v.InRelief ? "   RELIEF" : string.Empty);

        public static string Cars(in LotSnapshot s)
            => "Arriving " + s.CountCars(CarLotState.Arriving) + "  Waiting " + s.CountCars(CarLotState.WaitingToPark)
               + "  Driven " + s.CountCars(CarLotState.Driven) + "  Parked " + s.CountCars(CarLotState.Parked)
               + "  Station " + s.CountCars(CarLotState.InStation) + "  Pickup " + s.CountCars(CarLotState.PickupRequested)
               + "\nFree spots " + s.FreeSpots + "/" + s.TotalSpots;

        public static string Slot(in StationSlot slot, int number)
            => (slot.Kind == StationKind.Wash ? "Wash" : "Bay " + number) + ": " + slot.State
               + (slot.Car.IsValid ? " (" + slot.Car + ")" : string.Empty);

        /// <summary>One log line, for example "1:05 sent any (UnderTarget), 6 used".</summary>
        public static string Decision(in DirectorDecision d)
        {
            var sb = new StringBuilder(Clock(d.AtSeconds)).Append(' ');
            switch (d.Kind)
            {
                case DecisionKind.ReliefStarted: sb.Append("RELIEF started (").Append(d.Trigger).Append(')'); break;
                case DecisionKind.ReliefEnded: sb.Append("relief ended"); break;
                default:
                    sb.Append("sent ").Append(d.ArchetypeChosen ? d.Archetype.ToString() : "any")
                      .Append(" (").Append(d.Reason).Append("), ").Append(d.ArrivalsUsed).Append(" used, W ")
                      .Append(d.Workload.ToString("0.00")).Append(" T ").Append(d.Target.ToString("0.00"));
                    break;
            }
            return sb.ToString();
        }

        public static Color SlotColor(StationSlotState state)
        {
            switch (state)
            {
                case StationSlotState.Working: return new Color(0.3f, 0.8f, 0.35f);
                case StationSlotState.Blocked: return new Color(1f, 0.6f, 0.15f);
                case StationSlotState.Squatted: return new Color(0.9f, 0.25f, 0.25f);
                case StationSlotState.Reserved: return new Color(0.35f, 0.6f, 1f);
                default: return new Color(0.55f, 0.55f, 0.55f);
            }
        }
    }
}
