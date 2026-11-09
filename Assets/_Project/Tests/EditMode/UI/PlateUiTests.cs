using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI;
using UncannyValet.UI.Hud;
using UncannyValet.UI.Kit;

namespace UncannyValet.Tests.UI
{
    public class PlateUiTests
    {
        private static readonly TicketId T = new TicketId(1);
        private static readonly CarId C = new CarId(9);

        private EventBus _bus;
        private TicketBoard _board;
        private readonly List<TicketRow> _rows = new List<TicketRow>();

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _board = new TicketBoard(_bus);
        }

        [TearDown]
        public void TearDown() => _board.Dispose();

        private TicketRow Only()
        {
            _board.CollectRows(_rows);
            Assert.AreEqual(1, _rows.Count);
            return _rows[0];
        }

        [Test]
        public void PlateBeforeTicket_IsShown()
        {
            _bus.Publish(new CarPlateAssigned(C, "BKT 482"));
            _bus.Publish(new TicketOpened(T, C, 60f));
            Assert.AreEqual("BKT 482", Only().Plate);
            Assert.IsTrue(Only().HasPlate);
        }

        [Test]
        public void PlateAfterTicket_IsShown()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            Assert.IsFalse(Only().HasPlate);
            _bus.Publish(new CarPlateAssigned(C, "DGH 007"));
            Assert.AreEqual("DGH 007", Only().Plate);
        }

        [Test]
        public void ClosedTicketForgetsItsPlate()
        {
            _bus.Publish(new CarPlateAssigned(C, "BKT 482"));
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketClosed(T, C, 5f, 10));
            _bus.Publish(new TicketOpened(new TicketId(2), C, 60f));
            Assert.IsFalse(Only().HasPlate);
        }

        [Test]
        public void PlateWidthGrowsWithTheTextAndFitsTheBox()
        {
            Assert.AreEqual(PixelTextLayout.Width(7, 1) + 2 * (PlateLayout.Border + PlateLayout.PadX), PlateLayout.Width(7));
            Assert.Greater(PlateLayout.Width(7), PlateLayout.Width(6));
            Assert.AreEqual(PixelGlyphs.Height + 2 * (PlateLayout.Border + PlateLayout.PadY), PlateLayout.Height);
        }

        [Test]
        public void FlashPeriodShrinksAsPatienceRunsOut()
        {
            Assert.AreEqual(PlateLayout.SlowPeriod, PlateLayout.FlashPeriod(0f, false), 1e-5f);
            Assert.AreEqual(PlateLayout.FastPeriod, PlateLayout.FlashPeriod(1f, false), 1e-5f);
            Assert.AreEqual(PlateLayout.FastPeriod, PlateLayout.FlashPeriod(0.2f, true), 1e-5f);
            Assert.Less(PlateLayout.FlashPeriod(0.7f, false), PlateLayout.FlashPeriod(0.3f, false));
            Assert.AreEqual(PlateLayout.SlowPeriod, PlateLayout.FlashPeriod(-4f, false), 1e-5f);
        }
    }
}
