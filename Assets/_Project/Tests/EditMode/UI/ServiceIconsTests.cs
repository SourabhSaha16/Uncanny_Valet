using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
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

        private static TicketRow ArrivedRow(bool muddy, bool ev) => new TicketRow { Arrived = true }.Give(ev, muddy);

        [Test]
        public void CarNotArrivedYet_ShowsNothing()
        {
            var row = ArrivedRow(true, true);
            row.Arrived = false;
            Assert.AreEqual(0, ServiceIcons.Count(row));
        }

        [Test]
        public void ArrivedMuddyCar_ShowsMudSplat()
        {
            var row = ArrivedRow(true, false);
            Assert.AreEqual(1, ServiceIcons.Count(row));
            Assert.IsTrue(ServiceIcons.TryGet(row, 0, out var icon));
            Assert.AreEqual(PixelIcon.Mud, icon);
            Assert.IsFalse(ServiceIcons.TryGet(row, 1, out _));
        }

        [Test]
        public void ArrivedEv_ShowsPlugUntilFull()
        {
            var row = ArrivedRow(false, true);
            Assert.IsTrue(ServiceIcons.TryGet(row, 0, out var icon));
            Assert.AreEqual(PixelIcon.Plug, icon);
            row.Clear(ModifierId.NeedsCharge);
            Assert.AreEqual(0, ServiceIcons.Count(row));
        }

        [Test]
        public void MuddyEv_ShowsMudThenPlug_AndWashedDropsTheMud()
        {
            var row = ArrivedRow(true, true);
            Assert.AreEqual(2, ServiceIcons.Count(row));
            ServiceIcons.TryGet(row, 0, out var first);
            ServiceIcons.TryGet(row, 1, out var second);
            Assert.AreEqual(PixelIcon.Mud, first);
            Assert.AreEqual(PixelIcon.Plug, second);
            row.Clear(ModifierId.Muddy);
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
            _board = new TicketBoard(_bus, ModifierTestKit.Catalog());
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
            _bus.Publish(new TicketModifiersAssigned(T, C, ModifierTestKit.Of(false, true)));
            Assert.AreEqual(0, ServiceIcons.Count(Only()));
            _bus.Publish(new CarArrived(C));
            Assert.AreEqual(1, ServiceIcons.Count(Only()));
            _bus.Publish(new CarEnteredStation(C, StationKind.Wash));
            Assert.AreEqual(1, ServiceIcons.Count(Only()));
            _bus.Publish(new ModifierCleared(C, ModifierId.Muddy));
            Assert.AreEqual(0, ServiceIcons.Count(Only()));
            _bus.Publish(new TicketClosed(T, C, 0f, 0));
            Assert.IsNull(Only());
        }

        [Test]
        public void Replay_EvChargesToFull()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketModifiersAssigned(T, C, ModifierTestKit.Of(true, false)));
            _bus.Publish(new ModifierProgressChanged(C, ModifierId.NeedsCharge, 0.3f));
            _bus.Publish(new CarArrived(C));
            Assert.AreEqual(1, ServiceIcons.Count(Only()));
            _bus.Publish(new CarEnteredStation(C, StationKind.Charging));
            _bus.Publish(new ModifierProgressChanged(C, ModifierId.NeedsCharge, 1f));
            _bus.Publish(new ModifierCleared(C, ModifierId.NeedsCharge));
            Assert.AreEqual(0, ServiceIcons.Count(Only()));
        }

        [Test]
        public void Replay_CarThatNeedsNothing_NeverShowsAnIcon()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketModifiersAssigned(T, C, ModifierSet.None));
            _bus.Publish(new CarArrived(C));
            Assert.AreEqual(0, ServiceIcons.Count(Only()));
        }
    }
}
