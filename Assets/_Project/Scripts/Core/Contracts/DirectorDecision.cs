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
        /// <summary>True when a dilemma rule boosted the archetype that won the draw.</summary>
        public readonly bool HasRule;
        /// <summary>The dilemma rule (meaningful when <see cref="HasRule"/>; also set on a resolved entry).</summary>
        public readonly DilemmaKind Rule;
        /// <summary>The weight multiplier the rule applied; 1 without a rule.</summary>
        public readonly float Boost;
        /// <summary>Seconds a dilemma stayed open (resolved entries only).</summary>
        public readonly float ResolveSeconds;

        public DirectorDecision(DecisionKind kind, float atSeconds, ScheduleReason reason, ReliefTrigger trigger,
            CarArchetype archetype, bool archetypeChosen, int arrivalsUsed, float workload, float target,
            bool hasRule = false, DilemmaKind rule = default, float boost = 1f, float resolveSeconds = 0f)
        {
            Kind = kind; AtSeconds = atSeconds; Reason = reason; Trigger = trigger; Archetype = archetype;
            ArchetypeChosen = archetypeChosen; ArrivalsUsed = arrivalsUsed; Workload = workload; Target = target;
            HasRule = hasRule; Rule = rule; Boost = boost; ResolveSeconds = resolveSeconds;
        }
    }
}
