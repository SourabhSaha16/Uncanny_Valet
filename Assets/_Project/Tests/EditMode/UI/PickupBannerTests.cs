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

        private void Open(int id, int tag, int spot, bool requestPickup)
        {
            var car = new CarId(id);
            var ticket = new TicketId(id);
            _bus.Publish(new CarTagAssigned(car, tag));
            _bus.Publish(new TicketOpened(ticket, car, 60f));
            if (spot != 0) _bus.Publish(new CarParked(car, spot));
            if (requestPickup) _bus.Publish(new PickupRequested(ticket, car));
        }

        [Test]
        public void NoPickups_WritesNothing()
        {
            Open(1, 0, 2, false);
            Assert.AreEqual(0, _board.AppendPickups(_text, 3));
            Assert.AreEqual(string.Empty, _text.ToString());
        }

        [Test]
        public void OnePickup_NamesTheSpot()
        {
            Open(1, 1, 4, true);
            Assert.AreEqual(1, _board.AppendPickups(_text, 3));
            Assert.AreEqual("PICKUP: spot 4", _text.ToString());
        }

        [Test]
        public void UnknownSpot_ShowsPickupOnly()
        {
            Open(1, 2, 0, true);
            _board.AppendPickups(_text, 3);
            Assert.AreEqual("PICKUP", _text.ToString());
        }

        [Test]
        public void OnlyRequestedCars_AreListed()
        {
            Open(1, 0, 1, false);
            Open(2, 1, 2, true);
            _board.AppendPickups(_text, 3);
            Assert.AreEqual("PICKUP: spot 2", _text.ToString());
        }

        [Test]
        public void ThreePickups_OneLineEach()
        {
            Open(1, 0, 1, true);
            Open(2, 1, 2, true);
            Open(3, 2, 3, true);
            Assert.AreEqual(3, _board.AppendPickups(_text, 3));
            Assert.AreEqual("PICKUP: spot 1\nPICKUP: spot 2\nPICKUP: spot 3", _text.ToString());
        }

        [Test]
        public void FivePickups_CapAtThreeThenPlusTwoMore()
        {
            for (var i = 1; i <= 5; i++) Open(i, i - 1, i, true);
            Assert.AreEqual(4, _board.AppendPickups(_text, 3));
            StringAssert.EndsWith("\n+2 more", _text.ToString());
            StringAssert.DoesNotContain("spot 4", _text.ToString());
        }

        [Test]
        public void TagColoursComeBackOnePerPickupLine()
        {
            Open(1, 4, 1, true);
            Open(2, 7, 2, false);
            Open(3, 2, 3, true);
            var tags = new System.Collections.Generic.List<int> { 99 };
            _board.AppendPickups(_text, 3, tags);
            Assert.AreEqual(new[] { 4, 2 }, tags.ToArray());
        }

        [Test]
        public void ClosedTicket_LeavesTheList()
        {
            Open(1, 0, 1, true);
            _bus.Publish(new TicketClosed(new TicketId(1), new CarId(1), 0f, 0));
            Assert.AreEqual(0, _board.AppendPickups(_text, 3));
        }
    }
}
