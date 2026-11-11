using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;
using UncannyValet.Tests.Integration;

namespace UncannyValet.Tests.Patrons
{
    /// <summary>The Patrons side of the director's return nudge: only a patron still inside the diner can be moved.</summary>
    public class ReturnShiftTests
    {
        private const float Stay = 20f;

        private sealed class FakeClock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeTips : ITipCalculator
        {
            public int Calculate(in TipInput input) => 25;
            public TipBreakdown Explain(in TipInput input) => new TipBreakdown(25, 0, 0, 0, 25, TipZeroReason.None);
        }

        private EventBus _bus;
        private FakeClock _clock;
        private TicketRegistry _registry;
        private readonly List<PatronReturnShifted> _shifted = new List<PatronReturnShifted>();
        private int _summoned;
        private readonly CarId _car = new CarId(1);

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _clock = new FakeClock();
            _registry = new TicketRegistry(_bus, _clock, new FakeTips()) { CurbWalkTimeoutMinutes = 30f };
            _shifted.Clear();
            _summoned = 0;
            _bus.Subscribe<PatronReturnShifted>(e => _shifted.Add(e));
            _bus.Subscribe<PatronSummoned>(e => _summoned++);
            new FakeCarFactory(_bus).Spawn(_car, UnityEngine.Vector2.zero, 0f);
            _registry.Open(_car, new PatronRequest(Stay, 20, false, 60f));
            _bus.Publish(new CarArrived(_car));
            _bus.Publish(new CarParked(_car, 1));
        }

        private void Tick(float minute)
        {
            _clock.ElapsedMinutes = minute;
            _bus.Publish(new ShiftTick(minute));
        }

        [Test]
        public void ABroughtForwardReturnSummonsThePatronEarlier()
        {
            _bus.Publish(new ReturnShiftRequested(_car, -5f));
            Assert.AreEqual(1, _shifted.Count);
            Assert.AreEqual(-5f, _shifted[0].DeltaMinutes, 0.001f);
            Tick(Stay - 6f);
            Assert.AreEqual(0, _summoned);
            Tick(Stay - 5f);
            Assert.AreEqual(1, _summoned);
        }

        [Test]
        public void AHeldBackReturnSummonsThePatronLater()
        {
            _bus.Publish(new ReturnShiftRequested(_car, 5f));
            Tick(Stay + 1f);
            Assert.AreEqual(0, _summoned);
            Tick(Stay + 5f);
            Assert.AreEqual(1, _summoned);
        }

        [Test]
        public void AReturnIsNeverMovedIntoThePast()
        {
            _clock.ElapsedMinutes = 10f;
            _bus.Publish(new ReturnShiftRequested(_car, -50f));
            Assert.AreEqual(-10f, _shifted[0].DeltaMinutes, 0.001f, "clamped to now");
        }

        [Test]
        public void APatronWhoAlreadyLeftTheDinerCannotBeMoved()
        {
            Tick(Stay + 1f);
            Assert.AreEqual(1, _summoned);
            _bus.Publish(new ReturnShiftRequested(_car, 10f));
            Assert.AreEqual(0, _shifted.Count);
        }

        [Test]
        public void ACarWithoutATicketIsIgnored()
        {
            _bus.Publish(new ReturnShiftRequested(new CarId(99), -5f));
            Assert.AreEqual(0, _shifted.Count);
        }
    }
}
