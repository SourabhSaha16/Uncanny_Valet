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
            _board = new TicketBoard(_bus, ModifierTestKit.Catalog());
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
            _bus.Publish(new CarPlateAssigned(C, "BK 4"));
            _bus.Publish(new TicketOpened(T, C, 60f));
            Assert.AreEqual("BK 4", Only().Plate);
            Assert.IsTrue(Only().HasPlate);
        }

        [Test]
        public void PlateAfterTicket_IsShown()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            Assert.IsFalse(Only().HasPlate);
            _bus.Publish(new CarPlateAssigned(C, "DG 7"));
            Assert.AreEqual("DG 7", Only().Plate);
        }

        [Test]
        public void ClosedTicketForgetsItsPlate()
        {
            _bus.Publish(new CarPlateAssigned(C, "BK 4"));
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketClosed(T, C, 5f, 10));
            _bus.Publish(new TicketOpened(new TicketId(2), C, 60f));
            Assert.IsFalse(Only().HasPlate);
        }

        [Test]
        public void PlateTextUsesANarrowSpace()
        {
            Assert.AreEqual(2 * PixelTextLayout.Advance + PlateLayout.SpaceAdvance + PixelGlyphs.Width, PlateLayout.TextWidth("KT 4"));
            Assert.AreEqual(0, PlateLayout.TextWidth(""));
            Assert.AreEqual(0, PlateLayout.TextWidth(null));
        }

        [Test]
        public void PlateWidthFollowsTheCarButNeverBelowTheReadableMinimum()
        {
            var min = PlateLayout.MinWidth("KT 4");
            Assert.AreEqual(PlateLayout.TextWidth("KT 4") + 2 * PlateLayout.Border, min);
            Assert.AreEqual(40, PlateLayout.WidthFor("KT 4", 40));
            Assert.AreEqual(min, PlateLayout.WidthFor("KT 4", 10));
            Assert.AreEqual(min, PlateLayout.WidthFor("KT 4", 0));
            Assert.Greater(PlateLayout.Width("KT 4"), min);
            Assert.AreEqual(PixelGlyphs.Height + 2 * (PlateLayout.Border + PlateLayout.PadY), PlateLayout.Height);
        }
    }
}
