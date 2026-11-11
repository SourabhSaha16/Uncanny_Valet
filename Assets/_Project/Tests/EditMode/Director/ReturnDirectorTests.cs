using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>The shell around the return scheduler: builds its input, publishes the requests in game minutes.</summary>
    public class ReturnDirectorTests
    {
        private const float Elapsed = 100f;

        private EventBus _bus;
        private FakeClock _clock;
        private TuningStore _tuning;
        private DirectorSettings _settings;
        private ReturnDirector _director;
        private readonly List<ReturnShiftRequested> _requests = new List<ReturnShiftRequested>();

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _clock = new FakeClock();
            _clock.SetElapsed(Elapsed);
            _tuning = new TuningStore(_bus);
            _settings = new DirectorSettings();
            _director = new ReturnDirector(_bus, _clock, _tuning, _settings);
            _requests.Clear();
            _bus.Subscribe<ReturnShiftRequested>(e => _requests.Add(e));
        }

        [TearDown]
        public void TearDown() => _director.Dispose();

        private static LotSnapshot Lot(params float[] toPickup)
        {
            var b = new SnapshotBuilderForTests { NowSeconds = Elapsed };
            foreach (var t in toPickup) b.Car(CarLotState.Parked, toPickup: t, plannedStay: 40f);
            return b.Build();
        }

        [Test]
        public void TheLatePhaseAsksForEveryParkedReturnToComeForward()
        {
            _clock.SecondsRemaining = _settings.Returns.LateStartSeconds - 10f;
            _director.Tick(Lot(30f, 60f), 0f, false, DirectorPhase.Finale);
            Assert.AreEqual(2, _requests.Count);
            foreach (var r in _requests) Assert.Less(r.DeltaMinutes, 0f);
        }

        [Test]
        public void RequestsAreInGameMinutesNotSeconds()
        {
            _tuning.Seed(TuningKey.GameMinutesPerSecond, 2f);
            _clock.SecondsRemaining = _settings.Returns.LateStartSeconds - 10f;
            _director.Tick(Lot(60f), 0f, false, DirectorPhase.Finale);
            var seconds = _requests[0].DeltaMinutes / 2f;
            Assert.AreEqual(Elapsed + _settings.Returns.MinLeadSeconds, Elapsed + 60f + seconds, 0.01f);
        }

        [Test]
        public void ThePatronSentLastKeepsTheNextOneAwayFromTheCurb()
        {
            _bus.Publish(new PatronSummoned(new TicketId(9), new CarId(9)));
            _director.Tick(Lot(30f, 31f), 0f, false, DirectorPhase.BuildUp);
            Assert.AreEqual(1, _requests.Count);
            Assert.Greater(_requests[0].DeltaMinutes, 0f, "the closer pair is spread, never summoned together");
        }

        [Test]
        public void ANewShiftForgetsTheLastSummon()
        {
            _bus.Publish(new PatronSummoned(new TicketId(9), new CarId(9)));
            _bus.Publish(new ShiftStarted());
            _director.Tick(Lot(30f), 0f, false, DirectorPhase.BuildUp);
            Assert.AreEqual(0, _requests.Count);
        }

        [Test]
        public void UnderTargetNudgesOneReturnForward()
        {
            _director.Tick(Lot(30f), 0f, false, DirectorPhase.BuildUp);
            Assert.AreEqual(0, _requests.Count, "on target, nothing to do");
            _director.Tick(Lot(30f), -1f, false, DirectorPhase.BuildUp);
            Assert.AreEqual(1, _requests.Count);
        }
    }
}
