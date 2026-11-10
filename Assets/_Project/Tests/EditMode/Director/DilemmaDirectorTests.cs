using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    public class DilemmaDirectorTests
    {
        private EventBus _bus;
        private FakeClock _clock;
        private DirectorState _state;
        private DirectorSettings _settings;
        private DilemmaDirector _director;
        private SnapshotBuilderForTests _lot;
        private readonly List<DilemmaCreated> _created = new List<DilemmaCreated>();
        private readonly List<DilemmaResolved> _resolved = new List<DilemmaResolved>();
        private int _earlyReturns;

        [SetUp]
        public void SetUp()
        {
            // NUnit reuses one fixture instance for every test, so shared lists are reset here.
            _created.Clear();
            _resolved.Clear();
            _earlyReturns = 0;
            _bus = new EventBus();
            _clock = new FakeClock();
            _state = new DirectorState();
            _settings = new DirectorSettings();
            var rules = DilemmaRules.CreateAll(_settings.Rules);
            _director = new DilemmaDirector(_bus, _clock, new ArchetypePicker(new TestRandom(5), rules, _settings.Picker),
                new DilemmaGate(_settings.Gate), rules, _state);
            _bus.Subscribe<DilemmaCreated>(e => _created.Add(e));
            _bus.Subscribe<DilemmaResolved>(e => _resolved.Add(e));
            _bus.Subscribe<ReturnForwardWanted>(e => _earlyReturns++);
            _lot = new SnapshotBuilderForTests();
            _lot.Slot(StationKind.Wash, StationSlotState.Blocked);
            _lot.Slot(StationKind.Charging, StationSlotState.Free);
            _lot.Slot(StationKind.Charging, StationSlotState.Free);
        }

        [TearDown]
        public void TearDown() => _director.Dispose();

        private DilemmaContext Context(bool relief = false)
            => new DilemmaContext(ArchetypeWeights.Diner, ScheduleReason.UnderTarget, relief, 0f, false, 14);

        // Draws until a rule wins (the x3 Dirty boost does within a few draws), then spawns that car.
        private CarId MakeDilemma(float at)
        {
            _clock.SetElapsed(at);
            _lot.NowSeconds = at;
            for (var i = 1; i <= 60; i++)
            {
                var pick = _director.Choose(_lot.Build(), Context());
                var car = new CarId(100 + i);
                _bus.Publish(new CarSpawned(car, new TicketId(i)));
                if (pick.HasRule) return car;
            }
            Assert.Fail("no dilemma was made in 60 draws");
            return CarId.None;
        }

        [Test]
        public void ABoostedArrivalOpensADilemmaWithTheCarId()
        {
            var car = MakeDilemma(60f);
            Assert.AreEqual(1, _created.Count);
            Assert.AreEqual(DilemmaKind.WashBlocked, _created[0].Rule);
            Assert.AreEqual(car, _created[0].Car);
            Assert.AreEqual(1, _state.DilemmasMade);
        }

        [Test]
        public void ParkingTheNewCarAnywhereResolvesIt()
        {
            var car = MakeDilemma(60f);
            _bus.Publish(new CarParked(new CarId(999), 3));
            Assert.IsEmpty(_resolved, "another car does not count");
            _clock.SetElapsed(72f);
            _bus.Publish(new CarParked(car, 3));
            Assert.AreEqual(1, _resolved.Count);
            Assert.AreEqual(12f, _resolved[0].Seconds, 0.001f);
            Assert.AreEqual(1, _state.DilemmasResolved);
            Assert.AreEqual(12f, _state.AverageResolveSeconds, 0.001f);
        }

        [Test]
        public void EnteringAStationCountsAsParked()
        {
            var car = MakeDilemma(60f);
            _bus.Publish(new CarEnteredStation(car, StationKind.Wash));
            Assert.AreEqual(1, _resolved.Count);
        }

        [Test]
        public void TheBlockedStationFreeingResolvesIt()
        {
            MakeDilemma(60f);
            _lot = new SnapshotBuilderForTests();
            _lot.Slot(StationKind.Wash, StationSlotState.Free);
            _lot.NowSeconds = 70f;
            _director.Tick(_lot.Build());
            Assert.AreEqual(1, _resolved.Count);
            Assert.AreEqual(DilemmaKind.WashBlocked, _resolved[0].Rule);
            Assert.IsFalse(_state.HasOpenDilemma);
        }

        [Test]
        public void AtMostOneDilemmaIsOpenAndTheCooldownFollowsTheResolve()
        {
            var car = MakeDilemma(60f);
            for (var i = 0; i < 40; i++)
            {
                _director.Choose(_lot.Build(), Context());
                _bus.Publish(new CarSpawned(new CarId(500 + i), new TicketId(500 + i)));
            }
            Assert.AreEqual(1, _created.Count, "no second dilemma while one is open");
            _clock.SetElapsed(65f);
            _bus.Publish(new CarParked(car, 2));
            _lot.NowSeconds = 70f;
            _director.Tick(_lot.Build());
            Assert.Greater(_state.DilemmaCooldownLeft, 0f);
            for (var i = 0; i < 40; i++)
            {
                _director.Choose(_lot.Build(), Context());
                _bus.Publish(new CarSpawned(new CarId(700 + i), new TicketId(700 + i)));
            }
            Assert.AreEqual(1, _created.Count, "cooldown holds the next dilemma back");
        }

        [Test]
        public void NoDilemmaIsMadeDuringRelief()
        {
            for (var i = 0; i < 60; i++)
            {
                var pick = _director.Choose(_lot.Build(), Context(relief: true));
                Assert.IsFalse(pick.HasRule);
                Assert.AreEqual(CarArchetype.Plain, pick.Archetype);
            }
        }

        [Test]
        public void ALotTightPickPublishesTheEarlyReturnWish()
        {
            _lot = new SnapshotBuilderForTests { FreeSpots = 1, TotalSpots = 9 };
            _director.Choose(_lot.Build(), Context());
            Assert.AreEqual(1, _earlyReturns);
            Assert.IsTrue(_state.ReturnForwardWanted);
        }

        [Test]
        public void ANewShiftClearsTheOpenDilemma()
        {
            MakeDilemma(60f);
            _bus.Publish(new ShiftStarted());
            _bus.Publish(new CarParked(_created[0].Car, 1));
            Assert.IsEmpty(_resolved);
        }
    }
}
