using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Patron Director events, grouped in one file by domain.

    /// <summary>
    /// The director wants a patron car now. When <see cref="ArchetypeChosen"/> is false the spawner picks the car
    /// type itself (the pre-director random chances); otherwise it must send exactly <see cref="Archetype"/>.
    /// </summary>
    public readonly struct PatronScheduled : IGameEvent
    {
        public readonly CarArchetype Archetype;
        public readonly bool ArchetypeChosen;
        public readonly ScheduleReason Reason;
        public PatronScheduled(CarArchetype archetype, bool archetypeChosen, ScheduleReason reason)
        {
            Archetype = archetype; ArchetypeChosen = archetypeChosen; Reason = reason;
        }
    }

    /// <summary>The intensity curve moved into another phase of the shift.</summary>
    public readonly struct DirectorPhaseChanged : IGameEvent
    {
        public readonly DirectorPhase Phase;
        public DirectorPhaseChanged(DirectorPhase phase) { Phase = phase; }
    }

    /// <summary>The player is drowning: arrivals are held, then only plain cars are sent.</summary>
    public readonly struct ReliefStarted : IGameEvent
    {
        public readonly ReliefTrigger Trigger;
        public ReliefStarted(ReliefTrigger trigger) { Trigger = trigger; }
    }

    /// <summary>The relief plain cars were all sent; normal pacing resumes.</summary>
    public readonly struct ReliefEnded : IGameEvent { }

    /// <summary>A director-made dilemma opened: the boosted car type won the draw and the car arrived.</summary>
    public readonly struct DilemmaCreated : IGameEvent
    {
        public readonly DilemmaKind Rule;
        public readonly CarId Car;
        public DilemmaCreated(DilemmaKind rule, CarId car) { Rule = rule; Car = car; }
    }

    /// <summary>The open dilemma ended: its blocked station freed, or the new car was parked.</summary>
    public readonly struct DilemmaResolved : IGameEvent
    {
        public readonly DilemmaKind Rule;
        public readonly float Seconds;
        public DilemmaResolved(DilemmaKind rule, float seconds) { Rule = rule; Seconds = seconds; }
    }

    /// <summary>
    /// The lot is tight and the director would like one parked patron's return brought forward. Nothing acts on it
    /// until the return scheduler exists; the debug overlay shows it.
    /// </summary>
    public readonly struct ReturnForwardWanted : IGameEvent { }
}
