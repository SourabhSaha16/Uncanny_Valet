using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Panel rows of the arrival idle guard: there is always a car waiting, like an open ticket in Overcooked.</summary>
    public static class ArrivalRows
    {
        public const string Group = "Arrivals";

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.ArrivalIdleLimit, Group, "Longest wait with an empty queue (s)", 0.5f, 30f, 0.5f,
                "When no car is waiting to be parked, the next one is sent after this many seconds at most, even if the pacing would wait longer. While cars are queued nothing changes.",
                advanced: false),
            new TuningInfo(TuningKey.ArrivalIdleCutoff, Group, "No forced arrivals in the last (s)", 0f, 120f, 1f,
                "The empty-queue rule stops this many seconds before the shift ends, because a car sent that late cannot be served."),
        };
    }
}
