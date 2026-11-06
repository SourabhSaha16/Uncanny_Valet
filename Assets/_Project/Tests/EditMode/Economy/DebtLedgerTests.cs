using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;

namespace UncannyValet.Tests.Economy
{
    public class DebtLedgerTests
    {
        private EventBus _bus;
        private DebtLedger _ledger;
        private DebtChanged _last;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _ledger = new DebtLedger(_bus, 1500);
            _bus.Subscribe<DebtChanged>(e => _last = e);
        }

        private void Tip(int amount) => _bus.Publish(new TipEarned(new TicketId(1), amount));

        [Test]
        public void TipsAccumulateWithoutTouchingDebt()
        {
            Tip(20); Tip(30);
            Assert.AreEqual(50, _ledger.TipsTonight);
            Assert.AreEqual(1500, _ledger.DebtRemaining);
        }

        [Test]
        public void NightEnd_SubtractsTipsAndResets()
        {
            Tip(200);
            _bus.Publish(new NightEnded(1));
            Assert.AreEqual(1300, _ledger.DebtRemaining);
            Assert.AreEqual(0, _ledger.TipsTonight);
            Assert.AreEqual(1300, _last.DebtRemaining);
        }

        [Test]
        public void DebtNeverBelowZero()
        {
            Tip(5000);
            _bus.Publish(new NightEnded(1));
            Assert.AreEqual(0, _ledger.DebtRemaining);
        }

        [Test]
        public void NegativeTipIgnored()
        {
            Tip(-10);
            Assert.AreEqual(0, _ledger.TipsTonight);
        }

        [Test]
        public void TwoNights_Stack()
        {
            Tip(100); _bus.Publish(new NightEnded(1));
            Tip(50); _bus.Publish(new NightEnded(2));
            Assert.AreEqual(1350, _ledger.DebtRemaining);
        }
    }
}
