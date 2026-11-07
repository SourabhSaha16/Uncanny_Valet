using System.Text;
using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI;

namespace UncannyValet.Tests.UI
{
    public class PickupBannerTests
    {
        private EventBus _bus;
        private TicketBoard _board;
        private StringBuilder _text;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _board = new TicketBoard(_bus);
            _text = new StringBuilder();
        }

        [TearDown]
        public void TearDown() => _board.Dispose();

        private void Open(int id, char letter, int spot, bool requestPickup)
        {
            var car = new CarId(id);
            var ticket = new TicketId(id);
            _bus.Publish(new CarLetterAssigned(car, letter));
            _bus.Publish(new TicketOpened(ticket, car, 60f));
            if (spot != 0) _bus.Publish(new CarParked(car, spot));
            if (requestPickup) _bus.Publish(new PickupRequested(ticket, car));
        }

        [Test]
        public void NoPickups_WritesNothing()
        {
            Open(1, 'A', 2, false);
            Assert.AreEqual(0, _board.AppendPickups(_text, 3));
            Assert.AreEqual(string.Empty, _text.ToString());
        }

        [Test]
        public void OnePickup_NamesCarAndSpot()
        {
            Open(1, 'B', 4, true);
            Assert.AreEqual(1, _board.AppendPickups(_text, 3));
            Assert.AreEqual("PICKUP: car B from spot 4", _text.ToString());
        }

        [Test]
        public void UnknownSpot_ShowsLetterOnly()
        {
            Open(1, 'C', 0, true);
            _board.AppendPickups(_text, 3);
            Assert.AreEqual("PICKUP: car C", _text.ToString());
        }

        [Test]
        public void OnlyRequestedCars_AreListed()
        {
            Open(1, 'A', 1, false);
            Open(2, 'B', 2, true);
            _board.AppendPickups(_text, 3);
            Assert.AreEqual("PICKUP: car B from spot 2", _text.ToString());
        }

        [Test]
        public void ThreePickups_OneLineEach()
        {
            Open(1, 'A', 1, true);
            Open(2, 'B', 2, true);
            Open(3, 'C', 3, true);
            Assert.AreEqual(3, _board.AppendPickups(_text, 3));
            Assert.AreEqual("PICKUP: car A from spot 1\nPICKUP: car B from spot 2\nPICKUP: car C from spot 3", _text.ToString());
        }

        [Test]
        public void FivePickups_CapAtThreeThenPlusTwoMore()
        {
            for (var i = 1; i <= 5; i++) Open(i, (char)('A' + i - 1), i, true);
            Assert.AreEqual(4, _board.AppendPickups(_text, 3));
            StringAssert.EndsWith("\n+2 more", _text.ToString());
            StringAssert.DoesNotContain("car D", _text.ToString());
        }

        [Test]
        public void ClosedTicket_LeavesTheList()
        {
            Open(1, 'A', 1, true);
            _bus.Publish(new TicketClosed(new TicketId(1), new CarId(1), 0f, 0));
            Assert.AreEqual(0, _board.AppendPickups(_text, 3));
        }
    }
}
