using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Hud;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    /// <summary>Issue 173: an angry patron's late delivery shows a red "-N" at the pickup where the blessing pop would be.</summary>
    public class LateCursePopFlowTests
    {
        private static readonly CarId Car = new CarId(7);
        private static readonly TicketId Ticket = new TicketId(3);
        private EventBus _bus;
        private TransformCarFactory _cars;
        private GameObject _carObject;
        private BlessingPopSettings _settings;
        private BlessingPops _pops;
        private BlessingPopTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _cars = new TransformCarFactory();
            _carObject = new GameObject("Car");
            _carObject.transform.position = new Vector3(5f, 6f, 0f);
            _cars.Add(Car, _carObject.transform);
            _settings = new BlessingPopSettings();
            _pops = new BlessingPops(_settings);
            _tracker = new BlessingPopTracker(_bus, _cars, _pops, _settings, () => 1f);
        }

        [TearDown]
        public void TearDown()
        {
            _tracker.Dispose();
            Object.DestroyImmediate(_carObject);
        }

        private void Deliver(int curse, int blessing)
        {
            _bus.Publish(new CarDelivered(Ticket, Car));
            if (curse > 0) _bus.Publish(new CurseApplied(Ticket, curse));
            _bus.Publish(new TicketClosed(Ticket, Car, 3f, blessing));
            _bus.Publish(new PatronBoarded(Car));
        }

        [Test]
        public void AngryPatron_PopsARedMinusN_InsteadOfTheZero()
        {
            Deliver(3, 0);
            Assert.AreEqual(1, _pops.ActiveCount);
            Assert.AreEqual("-3", _pops[0].Text);
            Assert.AreEqual(BlessingPopKind.Curse, _pops[0].Kind);
            Assert.AreEqual(5f, _pops[0].World.x, 0.001f);
            Assert.AreEqual(6f + _settings.LiftWorldUnits, _pops[0].World.y, 0.001f);
        }

        [Test]
        public void OnTimePatron_StillPopsTheBlessing()
        {
            Deliver(0, 12);
            Assert.AreEqual("+12", _pops[0].Text);
            Assert.AreEqual(BlessingPopKind.Normal, _pops[0].Kind);
        }

        [Test]
        public void CurseOfOneTicket_DoesNotLeakIntoTheNext()
        {
            Deliver(2, 0);
            Deliver(0, 9);
            Assert.AreEqual("+9", _pops[1].Text);
        }

        [Test]
        public void VoidedTicket_NeverDelivered_PopsNothing()
        {
            _bus.Publish(new TicketClosed(Ticket, Car, 0f, 0));
            _bus.Publish(new PatronBoarded(Car));
            Assert.AreEqual(0, _pops.ActiveCount);
        }
    }
}
