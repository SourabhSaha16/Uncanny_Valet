namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Pure state machine of one pooled person. It only decides the next phase; the crowd turns phases into walks.
    /// Ambient: Idle (timer) to Roaming (timer) to Arriving to Inside (timer) to Roaming again; a person the F2 slider
    /// switches off goes back to Idle after a roam leg. Patron: ToDoor to Inside (until
    /// summoned) to ToCurb to Waiting, then Leaving (to board the car) or Idle (quietly gone).
    /// </summary>
    public sealed class PedestrianBrain
    {
        private float _timer;
        private float _insideSeconds;
        private float _roamSeconds;
        private bool _summoned;

        public PedestrianPhase Phase { get; private set; } = PedestrianPhase.Idle;

        /// <summary>True for a patron, false for an ambient person.</summary>
        public bool IsPatron { get; private set; }

        /// <summary>An ambient person whose wait is over and who should appear now.</summary>
        public bool SpawnDue => !IsPatron && Phase == PedestrianPhase.Idle && _timer <= 0f;

        /// <summary>Ambient: stay out of sight for this long, then appear.</summary>
        public void WaitToSpawn(float seconds)
        {
            IsPatron = false;
            Phase = PedestrianPhase.Idle;
            _timer = seconds;
        }

        /// <summary>Ambient: roam for <paramref name="roamSeconds"/>, then head for the door and spend <paramref name="insideSeconds"/> in the diner.</summary>
        public void BeginAmbient(float insideSeconds, float roamSeconds)
        {
            IsPatron = false;
            PlanVisit(insideSeconds, roamSeconds);
            _timer = roamSeconds;
            Phase = PedestrianPhase.Roaming;
        }

        /// <summary>Ambient: how long the next stay inside lasts and how long the roam after it lasts before the next door visit.</summary>
        public void PlanVisit(float insideSeconds, float roamSeconds)
        {
            _insideSeconds = insideSeconds;
            _roamSeconds = roamSeconds;
        }

        /// <summary>An ambient roaming person whose roam time is up: they decide to walk to the diner door.</summary>
        public bool EnterDue => !IsPatron && Phase == PedestrianPhase.Roaming && _timer <= 0f;

        /// <summary>Ambient: stop roaming and walk to the door.</summary>
        public void GoToDoor()
        {
            if (!IsPatron && Phase == PedestrianPhase.Roaming) Phase = PedestrianPhase.Arriving;
        }

        /// <summary>Patron: just got out of the car, walking to the door.</summary>
        public void BeginPatron()
        {
            IsPatron = true;
            _summoned = false;
            Phase = PedestrianPhase.ToDoor;
        }

        /// <summary>Patron: already inside the diner (their car arrived before this person existed).</summary>
        public void BeginPatronInside()
        {
            IsPatron = true;
            _summoned = false;
            Phase = PedestrianPhase.Inside;
        }

        /// <summary>Counts down the idle and inside timers of ambient people.</summary>
        public void Tick(float deltaSeconds)
        {
            if (IsPatron) return;
            if ((Phase == PedestrianPhase.Idle || Phase == PedestrianPhase.Roaming) && _timer > 0f) _timer -= deltaSeconds;
            else if (Phase == PedestrianPhase.Inside && (_timer -= deltaSeconds) <= 0f) CameOut();
        }

        // Out of the diner: roam again until the next decision to enter.
        private void CameOut()
        {
            Phase = PedestrianPhase.Roaming;
            _timer = _roamSeconds;
        }

        /// <summary>The current walk ended; moves to the next phase.</summary>
        public void ReachedRouteEnd()
        {
            switch (Phase)
            {
                case PedestrianPhase.Arriving:
                case PedestrianPhase.ToDoor:
                    Phase = PedestrianPhase.Inside;
                    _timer = _insideSeconds;
                    if (IsPatron && _summoned) Phase = PedestrianPhase.ToCurb;
                    break;
                case PedestrianPhase.ToCurb: Phase = PedestrianPhase.Waiting; break;
                case PedestrianPhase.Leaving: Phase = PedestrianPhase.Idle; _timer = 0f; break;
            }
        }

        /// <summary>Patron: the stay is over, come out and walk to the curb (as soon as they are inside).</summary>
        public void Summon()
        {
            if (!IsPatron) return;
            _summoned = true;
            if (Phase == PedestrianPhase.Inside) Phase = PedestrianPhase.ToCurb;
        }

        /// <summary>Patron: the ticket is over. Waiting patrons board the car when <paramref name="boardCar"/>; everyone else vanishes quietly.</summary>
        public void Dismiss(bool boardCar)
        {
            Phase = boardCar && Phase == PedestrianPhase.Waiting ? PedestrianPhase.Leaving : PedestrianPhase.Idle;
            _timer = 0f;
        }
    }
}
