using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>The arrival pacer's answer for one tick.</summary>
    public readonly struct PacerDecision
    {
        public readonly PacerAction Action;
        public readonly ScheduleReason Reason;
        public readonly HoldReason Hold;
        /// <summary>The car to send must be plain (relief).</summary>
        public readonly bool ForcePlain;
        /// <summary>Seconds until the next car at the current pace (0 when sending or unknown).</summary>
        public readonly float SecondsToNext;
        /// <summary>Relief began this tick.</summary>
        public readonly bool ReliefStarted;
        public readonly ReliefTrigger Trigger;
        /// <summary>Relief ended this tick (only when it had no plain cars to send).</summary>
        public readonly bool ReliefEnded;

        public PacerDecision(PacerAction action, ScheduleReason reason, HoldReason hold, bool forcePlain,
            float secondsToNext, bool reliefStarted, ReliefTrigger trigger, bool reliefEnded)
        {
            Action = action; Reason = reason; Hold = hold; ForcePlain = forcePlain; SecondsToNext = secondsToNext;
            ReliefStarted = reliefStarted; Trigger = trigger; ReliefEnded = reliefEnded;
        }

        public bool IsSpawn => Action == PacerAction.Spawn;
    }
}
