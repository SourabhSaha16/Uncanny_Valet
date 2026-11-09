using System.Collections.Generic;
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
        public void TagBeforeTicket_IsShown()
        {
            _bus.Publish(new CarTagAssigned(C, 1));
            _bus.Publish(new TicketOpened(T, C, 60f));
            Assert.AreEqual(1, Only().Tag);
        }

        [Test]
        public void TagAfterTicket_IsShownWithItsTimer()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarTagAssigned(C, 0));
            _bus.Publish(new TicketTimerChanged(T, C, TimerKind.Park, 0.5f));
            var row = Only();
            Assert.AreEqual(0, row.Tag);
            Assert.IsTrue(row.HasTimer);
            Assert.AreEqual(0.5f, row.Elapsed01, 0.001f);
        }

        [Test]
        public void ParkedStopsTimer_ThenPickupExpiresLate()
        {
            _bus.Publish(new CarTagAssigned(C, 0));
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarParked(C, 3));
            _bus.Publish(new TicketTimerStopped(T, C));
            var row = Only();
            Assert.AreEqual(TicketRowState.Parked, row.State);
            Assert.AreEqual(3, row.Spot);
            Assert.IsFalse(row.HasTimer);
            _bus.Publish(new PickupRequested(T, C));
            _bus.Publish(new TicketTimerExpired(T, C, TimerKind.Pickup));
            Assert.AreEqual(TicketRowState.PickupRequested, row.State);
            Assert.IsTrue(row.Expired);
            Assert.AreEqual(1, _board.PickupCount);
        }

        [Test]
        public void ThreeWaitingCars_AreListedByCar()
        {
            for (var i = 1; i <= 3; i++)
            {
                var car = new CarId(i);
                _bus.Publish(new TicketOpened(new TicketId(i), car, 60f));
                _bus.Publish(new CarTagAssigned(car, i - 1));
            }
            _board.CollectRows(_rows);
            Assert.AreEqual(new[] { 0, 1, 2 }, new[] { _rows[0].Tag, _rows[1].Tag, _rows[2].Tag });
        }

        [Test]
        public void InfoBox_WaitsForTheCarToArrive_AndHidesWhenItLeaves()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketTraitsAssigned(T, C, true, true));
            Assert.IsFalse(Only().ShowsInfoBox);
            _bus.Publish(new CarArrived(C));
            Assert.IsTrue(Only().ShowsInfoBox);
            _bus.Publish(new CarDeparted(C));
            Assert.IsFalse(Only().ShowsInfoBox);
        }

        [Test]
        public void ArrivalBeforeTheTicket_StillCounts()
        {
            _bus.Publish(new CarArrived(C));
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketTraitsAssigned(T, C, false, true));
            Assert.IsTrue(Only().ShowsInfoBox);
        }

        [Test]
        public void CarWithoutNeeds_HasNoInfoBox()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarArrived(C));
            _bus.Publish(new TicketTraitsAssigned(T, C, false, false));
            Assert.IsFalse(Only().ShowsInfoBox);
        }

        [Test]
        public void DirtMeterDrainsWhileWashing_ThenTheLineGoes()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarArrived(C));
            _bus.Publish(new TicketTraitsAssigned(T, C, false, true));
            Assert.AreEqual(1f, Only().Dirt01, 1e-4f);
            _bus.Publish(new CarCleanChanged(C, 0.75f));
            Assert.AreEqual(0.25f, Only().Dirt01, 1e-4f);
            _bus.Publish(new CarCleaned(C));
            Assert.IsFalse(Only().ShowsDirtMeter);
            Assert.IsFalse(Only().ShowsInfoBox);
        }

        [Test]
        public void ChargeMeterStaysForAnEv_EvenWhenFull()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarArrived(C));
            _bus.Publish(new TicketTraitsAssigned(T, C, true, false));
            _bus.Publish(new CarChargeChanged(C, 1f));
            Assert.IsTrue(Only().ShowsChargeMeter);
            Assert.IsTrue(Only().ShowsInfoBox);
            Assert.AreEqual(1f, Only().ChargeShown, 1e-4f);
        }

        [Test]
        public void EvTraits_ComeFromEvents()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketTraitsAssigned(T, C, true, false));
            Assert.IsTrue(Only().NeedsCharge);
            _bus.Publish(new CarChargeChanged(C, 0.45f));
            Assert.AreEqual(0.45f, Only().Charge01, 0.001f);
            _bus.Publish(new CarChargeChanged(C, 1f));
            Assert.IsFalse(Only().NeedsCharge);
        }

        [Test]
        public void DirtyTrait_ClearsWhenWashed()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketTraitsAssigned(T, C, false, true));
            Assert.IsTrue(Only().NeedsWash);
            _bus.Publish(new CarCleaned(C));
            Assert.IsFalse(Only().NeedsWash);
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

        [Test]
        public void Version_RisesWhenParkingOrPickupsChange()
        {
            var before = _board.Version;
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarParked(C, 2));
            _bus.Publish(new PickupRequested(T, C));
            Assert.Greater(_board.Version, before);
        }

        [Test]
        public void CollectRows_PutsPickupsAndLowPatienceFirst()
        {
            for (var i = 1; i <= 3; i++) _bus.Publish(new TicketOpened(new TicketId(i), new CarId(i), 60f));
            _bus.Publish(new TicketTimerChanged(new TicketId(2), new CarId(2), TimerKind.Park, 0.8f));
            _bus.Publish(new PickupRequested(new TicketId(3), new CarId(3)));
            _board.CollectRows(_rows);
            Assert.AreEqual(new[] { 3, 2, 1 }, new[] { _rows[0].Car, _rows[1].Car, _rows[2].Car });
        }
    }
}
