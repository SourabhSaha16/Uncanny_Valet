using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>One entry of the director's decision log, shown in the F3 overlay.</summary>
    public readonly struct DirectorDecision
    {
        public readonly DecisionKind Kind;
        public readonly float AtSeconds;
        public readonly ScheduleReason Reason;
        public readonly ReliefTrigger Trigger;
        public readonly CarArchetype Archetype;
        public readonly bool ArchetypeChosen;
        public readonly int ArrivalsUsed;
        public readonly float Workload;
        public readonly float Target;

        public DirectorDecision(DecisionKind kind, float atSeconds, ScheduleReason reason, ReliefTrigger trigger,
            CarArchetype archetype, bool archetypeChosen, int arrivalsUsed, float workload, float target)
        {
            Kind = kind; AtSeconds = atSeconds; Reason = reason; Trigger = trigger; Archetype = archetype;
            ArchetypeChosen = archetypeChosen; ArrivalsUsed = arrivalsUsed; Workload = workload; Target = target;
        }
    }
}
