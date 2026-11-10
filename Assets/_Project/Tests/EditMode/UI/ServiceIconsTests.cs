using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.UI;
using UncannyValet.UI.Hud;
using UncannyValet.UI.Kit;

namespace UncannyValet.Tests.UI
{
    public class ServiceIconsTests
    {
        private static TicketRow InStation(StationKind kind) => new TicketRow { HasStation = true, Station = kind };

        [Test]
        public void DirtyCarInWashSlot_ShowsDroplet()
        {
            var row = InStation(StationKind.Wash);
            row.IsDirty = true;
            Assert.IsTrue(ServiceIcons.TryGet(row, out var icon));
            Assert.AreEqual(PixelIcon.Wash, icon);
        }

        [Test]
        public void WashedCar_ShowsNoIcon()
        {
            var row = InStation(StationKind.Wash);
            row.IsDirty = true;
            row.Washed = true;
            Assert.IsFalse(ServiceIcons.TryGet(row, out _));
        }

        [Test]
        public void EvInChargingBay_ShowsPlugUntilFull()
        {
            var row = InStation(StationKind.Charging);
            row.IsEv = true;
            row.Charge01 = 0.5f;
            Assert.IsTrue(ServiceIcons.TryGet(row, out var icon));
            Assert.AreEqual(PixelIcon.Plug, icon);
            row.Charge01 = 1f;
            Assert.IsFalse(ServiceIcons.TryGet(row, out _));
        }

        [Test]
        public void NeedyCarOutsideAStation_ShowsNoIcon()
        {
            var row = new TicketRow { IsDirty = true, IsEv = true, Charge01 = 0.2f };
            Assert.IsFalse(ServiceIcons.TryGet(row, out _));
        }

        [Test]
        public void DirtyCarInChargingBay_ShowsNoWashIcon()
        {
            var row = InStation(StationKind.Charging);
            row.IsDirty = true;
            Assert.IsFalse(ServiceIcons.TryGet(row, out _));
        }
    }
}
