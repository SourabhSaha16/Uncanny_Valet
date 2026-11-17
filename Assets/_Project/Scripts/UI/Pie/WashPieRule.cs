using UncannyValet.Core.Events;

namespace UncannyValet.UI.Pie
{
    /// <summary>
    /// Pure rules of the wash pie: it shows while a car with an open wash modifier sits in the wash station, and a wedge is cut away
    /// clockwise from 12 o'clock as the wash progresses (full when the wash starts, empty when it is done), like the pickup pie.
    /// </summary>
    public static class WashPieRule
    {
        /// <summary>The wash modifier row the wash station is working on, or null (no open wash modifier, or the car is not in the wash).</summary>
        public static RowModifier Washing(TicketRow row)
            => row.InStation(StationKind.Wash) ? row.OpenAt(StationKind.Wash) : null;

        /// <summary>The pre-built wedge step for this wash progress: 0 = full pie, <paramref name="steps"/> = empty.</summary>
        public static int Step(float progress01, int steps) => PickupPieRule.Step(progress01, steps);

        /// <summary>True while the car is being washed and the wash is not yet done, so the pie shows.</summary>
        public static bool IsShown(TicketRow row, int steps)
        {
            var washing = Washing(row);
            return washing != null && Step(washing.Progress01, steps) < steps;
        }
    }
}
