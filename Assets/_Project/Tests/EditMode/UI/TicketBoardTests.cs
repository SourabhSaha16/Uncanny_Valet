using System.Text;
using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI;

namespace UncannyValet.Tests.UI
{
    public class TicketBoardTests
    {
        private static readonly TicketId T = new TicketId(1);
        private static readonly CarId C = new CarId(9);

        private EventBus _bus;
        private TicketBoard _board;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _board = new TicketBoard(_bus);
        }

        [TearDown]
        public void TearDown() => _board.Dispose();

        private string Lines()
        {
            var text = new StringBuilder();
            _board.AppendLines(text, 8);
            return text.ToString().TrimEnd();
        }

        [Test]
        public void LetterBeforeTicket_IsShown()
        {
            _bus.Publish(new CarLetterAssigned(C, 'B'));
            _bus.Publish(new TicketOpened(T, C, 60f));
            Assert.AreEqual("  B  Waiting", Lines());
        }

        [Test]
        public void LetterAfterTicket_IsShown()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarLetterAssigned(C, 'A'));
            _bus.Publish(new TicketTimerChanged(T, C, TimerKind.Park, 0.5f));
            Assert.AreEqual("  A  [Park 50%]", Lines());
        }

        [Test]
        public void ParkedStopsTimer_ThenPickupExpiresLate()
        {
            _bus.Publish(new CarLetterAssigned(C, 'A'));
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarParked(C, 3));
            _bus.Publish(new TicketTimerStopped(T, C));
            Assert.AreEqual("  A  Parked  in spot 3", Lines());
            _bus.Publish(new PickupRequested(T, C));
            _bus.Publish(new TicketTimerExpired(T, C, TimerKind.Pickup));
            Assert.AreEqual("  A  LATE: tip $0  in spot 3", Lines());
            Assert.AreEqual(1, _board.PickupCount);
        }

        [Test]
        public void ThreeQueuedCars_ShowALinesABC()
        {
            for (var i = 1; i <= 3; i++)
            {
                var car = new CarId(i);
                _bus.Publish(new TicketOpened(new TicketId(i), car, 60f));
                _bus.Publish(new CarLetterAssigned(car, (char)('A' + i - 1)));
            }
            var lines = Lines().Replace("\r", string.Empty).Split('\n');
            Assert.AreEqual(new[] { "  A  Waiting", "  B  Waiting", "  C  Waiting" }, lines);
        }

        [Test]
        public void EvTraits_ShowChargeFromEvents()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarLetterAssigned(C, 'A'));
            _bus.Publish(new TicketTraitsAssigned(T, C, true, false));
            Assert.AreEqual("  A  Waiting EV", Lines());
            _bus.Publish(new CarChargeChanged(C, 0.45f));
            Assert.AreEqual("  A  Waiting EV 45%", Lines());
        }

        [Test]
        public void DirtyTrait_ClearsToCleanWhenWashed()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarLetterAssigned(C, 'A'));
            _bus.Publish(new TicketTraitsAssigned(T, C, false, true));
            _bus.Publish(new CarCleanChanged(C, 0.5f));
            Assert.AreEqual("  A  Waiting DIRTY", Lines());
            _bus.Publish(new CarCleaned(C));
            Assert.AreEqual("  A  Waiting clean", Lines());
        }

        [Test]
        public void ParkExpiryClose_RemovesRow()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketTimerExpired(T, C, TimerKind.Park));
            _bus.Publish(new TicketClosed(T, C, 0f, 0));
            Assert.AreEqual(0, _board.Count);
            Assert.AreEqual(0, _board.PickupCount);
        }
    }
}
