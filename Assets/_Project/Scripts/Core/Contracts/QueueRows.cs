using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows for when a patron's stay starts after the car arrives in the queue.</summary>
    public static class QueueRows
    {
        public const string Group = "Queue and stay start";

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.QueueClearDistance, Group, "Queue clear distance (units)", 1f, 20f, 0.5f,
                "The patron's stay starts the first time their car rests this far from the nearest queue slot (in a bay, the wash, the aisle or at the lot edge). Cars left in the queue do not start it."),
            new TuningInfo(TuningKey.QueueMaxWaitMinutes, Group, "Longest wait in the queue (game minutes)", 0.5f, 20f, 0.5f,
                "A car that stays in the queue gets its stay started anyway after this long, so a forgotten car cannot stall the shift."),
        };
    }
}
