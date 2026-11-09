# Director-1 changelog

Source: docs/redesign/patron_director_spec.md (binding), rollout steps 1 and 2.

## director-1: lot snapshot, F3 overlay, workload, form, arrival pacer
Date: 2026-11-09
Files:
- Core (additive): events CarArchetype, ScheduleReason, DirectorPhase, ReliefTrigger, DirectorEvents (PatronScheduled, DirectorPhaseChanged, ReliefStarted, ReliefEnded); contracts StationSlotState, CarLotState, CarSnapshot, StationSlot, LotSnapshot, ILotSnapshotSource, DecisionKind, DirectorDecision, IDirectorView; 41 TuningKey entries (Dir*) and their TuningCatalog rows in a new "Director" group. No station events were added: CarEnteredStation/CarLeftStation (wash slot and charging bays), CarCleaned, CarFullyCharged, CarArrived, CarParked, CarLeftSpot, PickupRequested, TicketTimerChanged, TicketClosed and ComplaintFiled already cover everything the tracker needs.
- Director (new assembly UncannyValet.Director, references Core only): LotStateTracker (rule system, owns the snapshot), CarBook, CarRecord, SnapshotBuilder, SlotStateRule, WorkloadEstimator, WorkloadReading, FormTracker, FormMath, RecentTips, ArrivalPacer, ReliefGate, ReliefStage, PacerInput, PacerDecision, PacerAction, HoldReason, DirectorBrain, DirectorState, PatronDirector (MonoBehaviour shell), DirectorProfile, PhaseSpan, DirectorConfig, DirectorSettings, WorkloadSettings, PacingSettings, ReliefSettings, FormSettings, TuningRead, IDirectorRandom.
- Patrons: PatronSpawner executes PatronScheduled when DirEnabled is on and keeps the old fixed timer when it is off; PatronRequestRoller.Roll(progress, archetype); new ArchetypeTraits.
- UI: DirectorOverlay (F3, hidden while F2 is open) and DirectorOverlayText.
- Bootstrap: GameBootstrap (director fields, StartDirector), DirectorBootstrap, ShiftRandom (one seeded RNG per shift for spawner and director), TuningSeeder (seeds the director), asmdef reference.
- Editor: ProjectSetup (creates DirectorConfig and DinerDirectorProfile assets), BootstrapSceneBuilder (PatronDirector and DirectorOverlay objects, new GameBootstrap fields), asmdef reference.
- Tests (EditMode, new folder Tests/EditMode/Director): LotStateTrackerTests, LotStateStationTests, WorkloadEstimatorTests, FormTrackerTests, ArrivalPacerTests, DirectorProfileTests, DirectorBrainTests, DirectorOverlayTextTests, helpers; Patrons/ArchetypeTraitsTests; Integration/TuningSeederTests updated for the director keys; Tests asmdef reference.
Why: patron_director_spec.md section 7, steps 1 (snapshot and overlay, no behaviour change) and 2 (workload W and pressure P, form F, arrival pacing with idle spawn, under/on/over factors, relief, queue cap, 14 +/- 2 budget; archetype stays random). DirEnabled (default on, an F2 key) switches between the director and the old 15-25 s timer in the same build.
Tests: NOT run: Unity could not be launched in this sandbox, so compilation and the suite are unverified. New .meta files are not committed; Unity creates them on first import. The Bootstrap scene must be rebuilt with Uncanny Valet/Setup/Build Everything; until then GameBootstrap logs an error and falls back to the old timer.
