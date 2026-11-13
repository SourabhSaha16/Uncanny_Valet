using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;

namespace UncannyValet.Tests.Economy
{
    public class ShiftBlessingsTests
    {
        private sealed class FakeGoal : ILotGoal
        {
            public LotId Lot { get; set; } = new LotId(World.Midgard, LotKind.Training);
            public int BlessingThreshold { get; set; } = 30;
        }

        private EventBus _bus;
        private ShiftConfig _config;
        private ShiftBlessings _blessings;
        private FakeGoal _goal;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _config = ScriptableObject.CreateInstance<ShiftConfig>();
            _goal = new FakeGoal();
            _blessings = new ShiftBlessings(_bus, _config, () => _goal);
            _bus.Publish(new ShiftStarted());
        }

        [TearDown]
        public void TearDown()
        {
            _blessings.Dispose();
            Object.DestroyImmediate(_config);
        }

        private void Blessing(int amount) => _bus.Publish(new BlessingEarned(new TicketId(1), amount));
        private void Angry() => _bus.Publish(new TicketTimerExpired(new TicketId(1), new CarId(1), TimerKind.Pickup));

        [Test]
        public void Blessings_AreAdded_AndRaiseTheFavour()
        {
            Blessing(20);
            Blessing(40);
            Assert.AreEqual(60, _blessings.Blessings);
            Assert.AreEqual(3, _blessings.FavourMultiplier);
            Assert.AreEqual(3, _blessings.PeakFavour);
        }

        [Test]
        public void ZeroBlessing_ResetsTheFavour_WithoutACurse_AndKeepsThePeak()
        {
            Blessing(20);
            Blessing(20);
            Blessing(0);
            Assert.AreEqual(1, _blessings.FavourMultiplier);
            Assert.AreEqual(3, _blessings.PeakFavour);
            Assert.AreEqual(40, _blessings.Blessings);
            Assert.AreEqual(0, _blessings.Curses);
        }

        [Test]
        public void AngryPatron_IsACurseWithTheDefaultAmount_AndResetsTheFavour()
        {
            Blessing(20);
            Blessing(40);
            Angry();
            Assert.AreEqual(60 - _config.DefaultCurse, _blessings.Blessings);
            Assert.AreEqual(1, _blessings.Curses);
            Assert.AreEqual(1, _blessings.FavourMultiplier);
        }

        [Test]
        public void ACurseSourceMayAskForItsOwnAmount()
        {
            CurseApplied seen = default;
            _bus.Subscribe<CurseApplied>(e => seen = e);
            Blessing(50);
            _blessings.ApplyCurse(TicketId.None, 4);
            Assert.AreEqual(46, _blessings.Blessings);
            Assert.AreEqual(4, seen.Amount);
            Assert.AreEqual(TicketId.None, seen.Ticket);
        }

        [Test]
        public void Curse_PublishesAnEventWithTheAmount()
        {
            CurseApplied seen = default;
            _bus.Subscribe<CurseApplied>(e => seen = e);
            Angry();
            Assert.AreEqual(_config.DefaultCurse, seen.Amount);
            Assert.AreEqual(new TicketId(1), seen.Ticket);
        }

        [Test]
        public void Blessings_NeverDropBelowZero()
        {
            Angry();
            Angry();
            Assert.AreEqual(0, _blessings.Blessings);
            Assert.AreEqual(2, _blessings.Curses);
        }

        [Test]
        public void EveryChange_PublishesBlessingsChangedWithTheCurrentNumbers()
        {
            var seen = new List<BlessingsChanged>();
            _bus.Subscribe<BlessingsChanged>(seen.Add);
            Blessing(200);
            Angry();
            Assert.AreEqual(2, seen.Count);
            Assert.AreEqual(200, seen[0].Blessings);
            Assert.AreEqual(2, seen[0].FavourMultiplier);
            Assert.AreEqual(200 - _config.DefaultCurse, seen[1].Blessings);
            Assert.AreEqual(1, seen[1].Curses);
            Assert.AreEqual(1, seen[1].FavourMultiplier);
        }

        [Test]
        public void ShiftEnd_PublishesTheResultOnce_AndLaterEventsAreIgnored()
        {
            var results = new List<ShiftResult>();
            _bus.Subscribe<ShiftResultReady>(e => results.Add(e.Result));
            Blessing(320);
            Angry();
            _bus.Publish(new ShiftEnded());
            _bus.Publish(new ShiftEnded());
            Blessing(500);
            Angry();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(320 - _config.DefaultCurse, results[0].Blessings);
            Assert.AreEqual(1, results[0].Curses);
            Assert.AreEqual(2, results[0].PeakFavour);
            Assert.AreEqual(30, results[0].Threshold);
            Assert.AreEqual(320 - _config.DefaultCurse, _blessings.Blessings);
        }

        [Test]
        public void NewShift_ResetsEverything()
        {
            Blessing(100);
            Angry();
            _bus.Publish(new ShiftEnded());
            _bus.Publish(new ShiftStarted());
            Assert.AreEqual(0, _blessings.Blessings);
            Assert.AreEqual(0, _blessings.Curses);
            Assert.AreEqual(1, _blessings.FavourMultiplier);
            Assert.AreEqual(1, _blessings.PeakFavour);
            Blessing(10);
            Assert.AreEqual(10, _blessings.Blessings);
        }

        [Test]
        public void BeforeTheFirstShift_NothingCounts()
        {
            var bus = new EventBus();
            var blessings = new ShiftBlessings(bus, _config);
            bus.Publish(new BlessingEarned(new TicketId(1), 50));
            bus.Publish(new TicketTimerExpired(new TicketId(1), new CarId(1), TimerKind.Pickup));
            Assert.AreEqual(0, blessings.Blessings);
            Assert.AreEqual(0, blessings.Curses);
            blessings.Dispose();
        }

        [Test]
        public void Dispose_StopsListening()
        {
            _blessings.Dispose();
            Blessing(50);
            Assert.AreEqual(0, _blessings.Blessings);
        }
    }
}
