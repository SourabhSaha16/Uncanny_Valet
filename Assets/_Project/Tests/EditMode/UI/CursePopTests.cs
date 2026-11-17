using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Hud;
using UnityEngine;

namespace UncannyValet.Tests.UI
{
    public class CursePopTests
    {
        private EventBus _bus;
        private BlessingPops _pops;
        private BlessingPopSettings _settings;
        private CursePopTracker _tracker;
        private int _frame;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _settings = new BlessingPopSettings();
            _pops = new BlessingPops(_settings);
            _frame = 10;
            _tracker = new CursePopTracker(_bus, _pops, _settings, () => 1f, () => _frame);
        }

        [TearDown]
        public void TearDown() => _tracker.Dispose();

        private static PedestrianHit Hit(bool byPlayer = true) =>
            new PedestrianHit(new PedestrianId(1), false, new Vector2(3f, 4f), 5f, byPlayer, new CarId(1));

        private void Total(int blessings) => _bus.Publish(new BlessingsChanged(blessings, 1, 0));

        private BlessingPops.Pop First() => _pops[0];

        [Test]
        public void Lost_IsTheAmountOrWhatIsLeft()
        {
            Assert.AreEqual(2, CursePopRule.Lost(10, 2));
            Assert.AreEqual(5, CursePopRule.Lost(5, 5));
            Assert.AreEqual(3, CursePopRule.Lost(3, 5));
            Assert.AreEqual(0, CursePopRule.Lost(0, 5));
            Assert.AreEqual(0, CursePopRule.Lost(4, -1));
        }

        [Test]
        public void Format_IsMinusN()
        {
            Assert.AreEqual("-1", CursePopRule.Format(1));
            Assert.AreEqual("-5", CursePopRule.Format(5));
        }

        [Test]
        public void HitThenCurse_PopsOnceAtTheHit()
        {
            Total(10);
            _bus.Publish(Hit());
            _bus.Publish(new CurseApplied(TicketId.None, 2));
            Assert.AreEqual(1, _pops.ActiveCount);
            Assert.AreEqual("-2", First().Text);
            Assert.AreEqual(BlessingPopKind.Curse, First().Kind);
            Assert.AreEqual(3f, First().World.x);
        }

        [Test]
        public void CurseThenHit_PopsOnceAtTheHit()
        {
            Total(10);
            _bus.Publish(new CurseApplied(TicketId.None, 5));
            _bus.Publish(Hit());
            _bus.Publish(new BlessingsChanged(5, 1, 1));
            Assert.AreEqual(1, _pops.ActiveCount);
            Assert.AreEqual("-5", First().Text);
        }

        [Test]
        public void ClampedAtZero_ShowsWhatWasReallyLost()
        {
            Total(3);
            _bus.Publish(Hit());
            _bus.Publish(new CurseApplied(TicketId.None, 5));
            Assert.AreEqual("-3", First().Text);
        }

        [Test]
        public void NothingToLose_NoPop()
        {
            Total(0);
            _bus.Publish(Hit());
            _bus.Publish(new CurseApplied(TicketId.None, 5));
            Assert.AreEqual(0, _pops.ActiveCount);
        }

        [Test]
        public void HitWithoutACurse_NoPop()
        {
            Total(10);
            _bus.Publish(Hit());
            Assert.AreEqual(0, _pops.ActiveCount);
        }

        [Test]
        public void AStaleHit_DoesNotPairWithALaterCurse()
        {
            Total(10);
            _bus.Publish(Hit());
            _frame++;
            _bus.Publish(new CurseApplied(TicketId.None, 2));
            Assert.AreEqual(0, _pops.ActiveCount);
        }

        [Test]
        public void ATicketCurse_NeverPops()
        {
            Total(10);
            _bus.Publish(Hit());
            _bus.Publish(new CurseApplied(new TicketId(7), 5));
            Assert.AreEqual(0, _pops.ActiveCount);
        }

        [Test]
        public void AHitByAnotherCar_NeverPops()
        {
            Total(10);
            _bus.Publish(Hit(false));
            _bus.Publish(new CurseApplied(TicketId.None, 2));
            Assert.AreEqual(0, _pops.ActiveCount);
        }

        [Test]
        public void TwoHitsInTwoFrames_PopTwice()
        {
            Total(10);
            _bus.Publish(Hit());
            _bus.Publish(new CurseApplied(TicketId.None, 2));
            _frame++;
            _bus.Publish(Hit());
            _bus.Publish(new CurseApplied(TicketId.None, 2));
            Assert.AreEqual(2, _pops.ActiveCount);
        }
    }
}
