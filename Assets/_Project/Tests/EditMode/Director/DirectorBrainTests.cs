using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;
using UnityEngine;

namespace UncannyValet.Tests.Director
{
    public class DirectorBrainTests
    {
        private const float Dt = 0.5f;

        private EventBus _bus;
        private TuningStore _tuning;
        private FakeClock _clock;
        private FakeLot _lot;
        private DirectorState _state;
        private DirectorBrain _brain;
        private FormTracker _form;
        private DirectorConfig _config;
        private DirectorProfile _profile;
        private readonly List<PatronScheduled> _scheduled = new List<PatronScheduled>();
        private readonly List<DirectorPhase> _phases = new List<DirectorPhase>();

        [SetUp]
        public void SetUp()
        {
            // NUnit reuses one fixture instance for every test, so the recorded events must be reset here.
            _scheduled.Clear();
            _phases.Clear();
            _bus = new EventBus();
            _tuning = new TuningStore(_bus);
            _config = ScriptableObject.CreateInstance<DirectorConfig>();
            _profile = ScriptableObject.CreateInstance<DirectorProfile>();
            _config.SeedInto(_tuning, _profile);
            _clock = new FakeClock();
            _lot = new FakeLot(_clock);
            _state = new DirectorState();
            var settings = new DirectorSettings();
            settings.Refresh(_tuning);
            _form = new FormTracker(_bus, settings.Form, _clock, null, 4);
            var pacer = new ArrivalPacer(new MidRandom(), settings.Pacing, new ReliefGate(settings.Relief));
            _brain = new DirectorBrain(_bus, _tuning, _clock, _lot, _profile, _state, settings, _form, pacer);
            _bus.Subscribe<PatronScheduled>(e => _scheduled.Add(e));
            _bus.Subscribe<DirectorPhaseChanged>(e => _phases.Add(e.Phase));
        }

        [TearDown]
        public void TearDown()
        {
            _brain.Dispose();
            _form.Dispose();
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_profile);
        }

        private void Run(float seconds)
        {
            for (var t = 0f; t < seconds; t += Dt)
            {
                _clock.SetElapsed(_clock.ShiftLengthSeconds - _clock.SecondsRemaining + Dt);
                _brain.Tick(Dt);
            }
        }

        [Test]
        public void IdleLotGetsAScheduledCarWithoutAChosenArchetype()
        {
            Run(7f);
            Assert.AreEqual(1, _scheduled.Count);
            Assert.AreEqual(ScheduleReason.Idle, _scheduled[0].Reason);
            Assert.IsFalse(_scheduled[0].ArchetypeChosen, "step 2: the spawner still rolls the type");
            Assert.AreEqual(1, _state.DecisionCount);
        }

        [Test]
        public void FirstTickPublishesTheBuildUpPhase()
        {
            Run(2f);
            Assert.AreEqual(new[] { DirectorPhase.BuildUp }, _phases.ToArray());
            Assert.AreEqual(DirectorPhase.BuildUp, _state.Phase);
        }

        [Test]
        public void CarSpawnedCountsAgainstTheBudgetAndRestartsTheCountdown()
        {
            Run(7f);
            _bus.Publish(new CarSpawned(new CarId(1), new TicketId(1)));
            Assert.AreEqual(1, _state.ArrivalsUsed);
            Run(3f);
            Assert.AreEqual(1, _scheduled.Count, "the next car needs a fresh countdown");
        }

        [Test]
        public void TurningTheDirectorOffStopsScheduling()
        {
            _tuning.Set(TuningKey.DirEnabled, 0f);
            Run(30f);
            Assert.AreEqual(0, _scheduled.Count);
            Assert.IsFalse(_state.Active);
        }

        [Test]
        public void NothingHappensWhileTheClockIsStopped()
        {
            _clock.IsRunning = false;
            Run(30f);
            Assert.AreEqual(0, _scheduled.Count);
        }

        [Test]
        public void DrowningLotStartsReliefAndLogsIt()
        {
            var b = new SnapshotBuilderForTests();
            for (var i = 0; i < 6; i++) b.Car(CarLotState.PickupRequested, patience: 0.5f);
            _lot.Next = b.Build();
            var relief = 0;
            _bus.Subscribe<ReliefStarted>(e => relief++);
            Run(1f);
            Assert.AreEqual(1, relief);
            Assert.IsTrue(_state.InRelief);
            Assert.AreEqual(DecisionKind.ReliefStarted, _state.GetDecision(0).Kind);
            Assert.AreEqual(0, _scheduled.Count);
        }

        [Test]
        public void NewShiftClearsTheCountersAndTheLog()
        {
            Run(7f);
            _bus.Publish(new CarSpawned(new CarId(1), new TicketId(1)));
            _bus.Publish(new ShiftStarted());
            Assert.AreEqual(0, _state.ArrivalsUsed);
            Assert.AreEqual(0, _state.DecisionCount);
        }

        [Test]
        public void DecisionLogKeepsTheLatestFive()
        {
            for (var i = 0; i < 8; i++)
                _state.Log(new DirectorDecision(DecisionKind.Scheduled, i, ScheduleReason.OnTarget, ReliefTrigger.Workload,
                    CarArchetype.Plain, false, i, 0f, 0f));
            Assert.AreEqual(5, _state.DecisionCount);
            Assert.AreEqual(7f, _state.GetDecision(0).AtSeconds);
            Assert.AreEqual(3f, _state.GetDecision(4).AtSeconds);
        }
    }
}
