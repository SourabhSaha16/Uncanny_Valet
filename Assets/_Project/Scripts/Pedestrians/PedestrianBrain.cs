namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Pure state machine of one pooled person. It only decides the next phase; the crowd turns phases into walks.
    /// Ambient: Idle (timer) to Arriving to Inside (timer) to Leaving to Idle. Patron: ToDoor to Inside (until
    /// summoned) to ToCurb to Waiting, then Leaving (to board the car) or Idle (quietly gone).
    /// </summary>
    public sealed class PedestrianBrain
    {
        private float _timer;
        private float _insideSeconds;
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

        /// <summary>Ambient: walk in from the road side; spend <paramref name="insideSeconds"/> in the diner.</summary>
        public void BeginAmbient(float insideSeconds)
        {
            IsPatron = false;
            _insideSeconds = insideSeconds;
            Phase = PedestrianPhase.Arriving;
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
            if (Phase == PedestrianPhase.Idle && _timer > 0f) _timer -= deltaSeconds;
            else if (Phase == PedestrianPhase.Inside && (_timer -= deltaSeconds) <= 0f) Phase = PedestrianPhase.Leaving;
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
