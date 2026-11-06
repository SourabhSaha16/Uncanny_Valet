using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;
using UncannyValet.Tests.Integration;

namespace UncannyValet.Tests.Patrons
{
    public class TicketRegistryNightTests
    {
        private sealed class FakeClock : IGameClock
        {
            public int Night => 1;
            public float MinuteOfNight { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeTips : ITipCalculator
        {
            public int Calculate(in TipInput input) => 25;
        }

        private EventBus _bus;
        private FakeClock _clock;
        private FakeCarFactory _cars;
        private TicketRegistry _registry;
        private List<IGameEvent> _log;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus(); _clock = new FakeClock();
            _cars = new FakeCarFactory(_bus);
            _registry = new TicketRegistry(_bus, _clock, new FakeTips());
            new PatronDeparture(_bus, _cars);
            _log = new List<IGameEvent>();
            _bus.Subscribe<TicketClosed>(e => _log.Add(e));
            _bus.Subscribe<TipEarned>(e => _log.Add(e));
            _bus.Subscribe<TicketTimerStopped>(e => _log.Add(e));
            _bus.Subscribe<CarDeparted>(e => _log.Add(e));
        }

        private Ticket Open(int car)
        {
            _cars.Spawn(new CarId(car), UnityEngine.Vector2.zero, 0f);
            return _registry.Open(new CarId(car), new PatronRequest(20f, 20, false, 45f, 60f));
        }

        private int Count<T>() where T : IGameEvent
        {
            var n = 0;
            foreach (var e in _log) if (e is T) n++;
            return n;
        }

        [Test]
        public void NightEnded_ClosesEveryOpenTicketWithZeroTipAndDepartsCars()
        {
            var waiting = Open(1);
            var parked = Open(2);
            _bus.Publish(new CarParked(parked.CarId, 1));

            _bus.Publish(new NightEnded(1));

            Assert.AreEqual(TicketState.Closed, waiting.State);
            Assert.AreEqual(TicketState.Closed, parked.State);
            Assert.AreEqual(0, _registry.OpenCount);
            Assert.AreEqual(2, Count<TicketClosed>());
            Assert.AreEqual(0, Count<TipEarned>());
            Assert.AreEqual(2, Count<CarDeparted>());
            Assert.AreEqual(0, _cars.Alive);
            foreach (var e in _log)
                if (e is TicketClosed c) { Assert.AreEqual(0, c.Tip); Assert.AreEqual(0f, c.WaitMinutes); }
        }

        [Test]
        public void NightEnded_StopsRunningTimers()
        {
            Open(1);
            _bus.Publish(new NightEnded(1));
            Assert.AreEqual(1, Count<TicketTimerStopped>());
        }

        [Test]
        public void NightEndedTwice_AndLateCurb_DoNotCloseAgain()
        {
            var t = Open(1);
            _bus.Publish(new CarParked(t.CarId, 1));
            _clock.MinuteOfNight = 30f;
            _bus.Publish(new NightTick(1, 30f));
            _bus.Publish(new NightEnded(1));
            _bus.Publish(new NightEnded(1));
            _bus.Publish(new CarAtCurb(t.CarId));

            Assert.AreEqual(1, Count<TicketClosed>());
            Assert.AreEqual(0, Count<TipEarned>());
            Assert.AreEqual(1, Count<CarDeparted>());
        }

        [Test]
        public void NextNight_StartsCleanAndLettersRestartFromA()
        {
            Open(1); Open(2); var third = Open(3);
            Assert.AreEqual('C', third.Letter);
            _bus.Publish(new NightEnded(1));
            _bus.Publish(new NightStarted(2));

            Assert.AreEqual(0, _registry.OpenCount);
            Assert.AreEqual(0, _registry.AwaitingParkCount);
            Assert.AreEqual(0, _cars.Alive);
            Assert.AreEqual('A', Open(4).Letter);
            Assert.AreEqual('B', Open(5).Letter);
            Assert.AreEqual(2, _registry.AwaitingParkCount);
        }
    }
}
