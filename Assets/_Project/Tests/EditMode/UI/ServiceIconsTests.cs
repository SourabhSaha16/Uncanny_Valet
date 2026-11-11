using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI;
using UncannyValet.UI.Hud;
using UncannyValet.UI.Kit;

namespace UncannyValet.Tests.UI
{
    public class ServiceIconsTests
    {
        private static readonly CarId C = new CarId(1);
        private static readonly TicketId T = new TicketId(1);
        private EventBus _bus;
        private TicketBoard _board;

        private static TicketRow ArrivedRow(bool dirty, bool ev)
        {
            return new TicketRow { Arrived = true, IsDirty = dirty, IsEv = ev, Charge01 = ev ? 0.4f : TicketRow.UnknownCharge };
        }

        [Test]
        public void CarNotArrivedYet_ShowsNothing()
        {
            var row = ArrivedRow(true, true);
            row.Arrived = false;
            Assert.AreEqual(0, ServiceIcons.Count(row));
        }

        [Test]
        public void ArrivedDirtyCar_ShowsDroplet()
        {
            var row = ArrivedRow(true, false);
            Assert.AreEqual(1, ServiceIcons.Count(row));
            Assert.IsTrue(ServiceIcons.TryGet(row, 0, out var icon));
            Assert.AreEqual(PixelIcon.Wash, icon);
            Assert.IsFalse(ServiceIcons.TryGet(row, 1, out _));
        }

        [Test]
        public void ArrivedEv_ShowsPlugUntilFull()
        {
            var row = ArrivedRow(false, true);
            Assert.IsTrue(ServiceIcons.TryGet(row, 0, out var icon));
            Assert.AreEqual(PixelIcon.Plug, icon);
            row.Charge01 = 1f;
            Assert.AreEqual(0, ServiceIcons.Count(row));
        }

        [Test]
        public void DirtyEv_ShowsDropletThenPlug_AndWashedDropsTheDroplet()
        {
            var row = ArrivedRow(true, true);
            Assert.AreEqual(2, ServiceIcons.Count(row));
            ServiceIcons.TryGet(row, 0, out var first);
            ServiceIcons.TryGet(row, 1, out var second);
            Assert.AreEqual(PixelIcon.Wash, first);
            Assert.AreEqual(PixelIcon.Plug, second);
            row.Washed = true;
            ServiceIcons.TryGet(row, 0, out var left);
            Assert.AreEqual(PixelIcon.Plug, left);
        }

        [Test]
        public void CarNeedingNothing_ShowsNoIcon()
        {
            Assert.AreEqual(0, ServiceIcons.Count(ArrivedRow(false, false)));
        }

        [Test]
        public void StationPositionDoesNotMatter()
        {
            var row = ArrivedRow(true, false);
            row.HasStation = true;
            row.Station = StationKind.Wash;
            Assert.AreEqual(1, ServiceIcons.Count(row));
            row.HasStation = false;
            Assert.AreEqual(1, ServiceIcons.Count(row));
        }

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
            var rows = new System.Collections.Generic.List<TicketRow>();
            _board.CollectRows(rows);
            return rows.Count == 0 ? null : rows[0];
        }

        [Test]
        public void Replay_ArriveCarryWashClose()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketTraitsAssigned(T, C, false, true));
            Assert.AreEqual(0, ServiceIcons.Count(Only()));
            _bus.Publish(new CarArrived(C));
            Assert.AreEqual(1, ServiceIcons.Count(Only()));
            _bus.Publish(new CarEnteredStation(C, StationKind.Wash));
            Assert.AreEqual(1, ServiceIcons.Count(Only()));
            _bus.Publish(new CarCleaned(C));
            Assert.AreEqual(0, ServiceIcons.Count(Only()));
            _bus.Publish(new TicketClosed(T, C, 0f, 0));
            Assert.IsNull(Only());
        }

        [Test]
        public void Replay_EvChargesToFull()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketTraitsAssigned(T, C, true, false));
            _bus.Publish(new CarChargeChanged(C, 0.3f));
            _bus.Publish(new CarArrived(C));
            Assert.AreEqual(1, ServiceIcons.Count(Only()));
            _bus.Publish(new CarEnteredStation(C, StationKind.Charging));
            _bus.Publish(new CarChargeChanged(C, 1f));
            _bus.Publish(new CarFullyCharged(C));
            Assert.AreEqual(0, ServiceIcons.Count(Only()));
        }

        [Test]
        public void Replay_CarThatNeedsNothing_NeverShowsAnIcon()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketTraitsAssigned(T, C, false, false));
            _bus.Publish(new CarArrived(C));
            Assert.AreEqual(0, ServiceIcons.Count(Only()));
        }
    }
}
