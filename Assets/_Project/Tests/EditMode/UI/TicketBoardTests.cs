using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.UI;
using UncannyValet.UI.Mood;
using UncannyValet.UI.Status;

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
            _bus.Publish(new TicketTimerChanged(T, C, TimerKind.Pickup, 0.5f));
            var row = Only();
            Assert.AreEqual(0, row.Tag);
            Assert.IsTrue(row.HasTimer);
            Assert.AreEqual(0.5f, row.Elapsed01, 0.001f);
        }

        private List<TicketRow> KeyRows()
        {
            var rows = new List<TicketRow>();
            _board.CollectKeyBoardRows(rows);
            return rows;
        }

        [Test]
        public void KeyBoard_ListsAnUnplacedCarAsSoonAsItsPatronAsks()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarArrived(C));
            Assert.AreEqual(0, KeyRows().Count);
            _bus.Publish(new PickupRequested(T, C));
            Assert.AreEqual(1, KeyRows().Count);
            Assert.IsTrue(Only().Placed);
        }

        [Test]
        public void KeyBoard_ListsACarOnlyOnceParked_AndKeepsItUntilDelivered()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarArrived(C));
            Assert.AreEqual(0, KeyRows().Count);
            Assert.AreEqual(1, _board.Count);
            _bus.Publish(new CarParked(C, 3));
            Assert.AreEqual(1, KeyRows().Count);
            _bus.Publish(new CarLeftSpot(C, 3));
            _bus.Publish(new PickupRequested(T, C));
            Assert.AreEqual(1, KeyRows().Count);
            _bus.Publish(new TicketClosed(T, C, 0f, 0));
            Assert.AreEqual(0, KeyRows().Count);
        }

        [Test]
        public void KeyBoard_ListsACarParkedInAStation_AndOneParkedBeforeItsTicket()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarEnteredStation(C, StationKind.Charging));
            Assert.AreEqual(1, KeyRows().Count);
            var early = new CarId(5);
            _bus.Publish(new CarParked(early, 2));
            _bus.Publish(new TicketOpened(new TicketId(2), early, 60f));
            Assert.AreEqual(2, KeyRows().Count);
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
            _bus.Publish(new TicketModifiersAssigned(T, C, ModifierTestKit.Of(true, true)));
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
            _bus.Publish(new TicketModifiersAssigned(T, C, ModifierTestKit.Of(false, true)));
            Assert.IsTrue(Only().ShowsInfoBox);
        }

        [Test]
        public void CarWithoutNeeds_HasNoInfoBox()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarArrived(C));
            _bus.Publish(new TicketModifiersAssigned(T, C, ModifierSet.None));
            Assert.IsFalse(Only().ShowsInfoBox);
        }

        [Test]
        public void MudMeterFillsWhileWashing_ThenTheLineGoes()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarArrived(C));
            _bus.Publish(new TicketModifiersAssigned(T, C, ModifierTestKit.Of(false, true)));
            _bus.Publish(new ModifierProgressChanged(C, ModifierId.Muddy, 0f));
            Assert.AreEqual(0f, Only().Find(ModifierId.Muddy).Progress01, 1e-4f);
            _bus.Publish(new ModifierProgressChanged(C, ModifierId.Muddy, 0.75f));
            Assert.AreEqual(0.75f, Only().Find(ModifierId.Muddy).Progress01, 1e-4f);
            _bus.Publish(new ModifierCleared(C, ModifierId.Muddy));
            Assert.IsFalse(Only().NeedsService);
            Assert.IsFalse(Only().ShowsInfoBox);
        }

        [Test]
        public void ModifierTraits_ComeFromEvents_WithTheirStartingProgress()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketModifiersAssigned(T, C, ModifierTestKit.Of(true, false)));
            Assert.IsTrue(Only().NeedsService);
            Assert.AreEqual(0.2f, Only().Find(ModifierId.NeedsCharge).Progress01, 0.001f, "starts at the definition's progress");
            _bus.Publish(new ModifierProgressChanged(C, ModifierId.NeedsCharge, 0.45f));
            Assert.AreEqual(0.45f, Only().Find(ModifierId.NeedsCharge).Progress01, 0.001f);
            _bus.Publish(new ModifierCleared(C, ModifierId.NeedsCharge));
            Assert.IsFalse(Only().NeedsService);
        }

        [Test]
        public void TwoModifiers_AreTrackedSeparately()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketModifiersAssigned(T, C, ModifierTestKit.Of(true, true)));
            _bus.Publish(new ModifierCleared(C, ModifierId.Muddy));
            Assert.IsTrue(Only().NeedsService, "the charge is still open");
            Assert.IsTrue(Only().Find(ModifierId.Muddy).IsCleared);
            Assert.IsFalse(Only().Find(ModifierId.NeedsCharge).IsCleared);
            _bus.Publish(new CarParked(C, 1));
            Assert.IsFalse(Only().Find(ModifierId.Muddy).ShowDone, "parking ends the done word");
        }

        [Test]
        public void ExpiryClose_RemovesRow()
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new TicketTimerExpired(T, C, TimerKind.Pickup));
            _bus.Publish(new TicketClosed(T, C, 0f, 0));
            Assert.AreEqual(0, _board.Count);
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
            _bus.Publish(new TicketTimerChanged(new TicketId(2), new CarId(2), TimerKind.Pickup, 0.8f));
            _bus.Publish(new PickupRequested(new TicketId(3), new CarId(3)));
            _board.CollectRows(_rows);
            Assert.AreEqual(new[] { 3, 2, 1 }, new[] { _rows[0].Car, _rows[1].Car, _rows[2].Car });
        }

        [Test]
        public void PlateShowsOnlyOnceTheCarIsParked()
        {
            _bus.Publish(new CarPlateAssigned(C, "KT 4"));
            _bus.Publish(new TicketOpened(T, C, 60f));
            Assert.IsFalse(Only().ShowsPlate, "queued or driving in");
            _bus.Publish(new CarArrived(C));
            Assert.IsFalse(Only().ShowsPlate, "arrived but not parked");
            _bus.Publish(new CarParked(C, 3));
            Assert.IsTrue(Only().ShowsPlate);
        }

        [Test]
        public void PlateShowsOnceTheCarEntersAStation()
        {
            _bus.Publish(new CarPlateAssigned(C, "KT 4"));
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarEnteredStation(C, StationKind.Wash));
            Assert.IsTrue(Only().ShowsPlate);
        }

        private static readonly CarStatusResolver Status = CarStatusResolver.CreateDefault(new EmotionRule(0.6f, 0.85f));

        private void OpenCar(bool ev, bool muddy)
        {
            _bus.Publish(new TicketOpened(T, C, 60f));
            _bus.Publish(new CarArrived(C));
            _bus.Publish(new TicketModifiersAssigned(T, C, ModifierTestKit.Of(ev, muddy)));
        }

        [Test]
        public void WashThenPark_StatusReturnsToParked()
        {
            OpenCar(false, true);
            _bus.Publish(new CarEnteredStation(C, StationKind.Wash));
            _bus.Publish(new ModifierProgressChanged(C, ModifierId.Muddy, 0.5f));
            Assert.AreEqual(CarStatus.Servicing, Status.Resolve(Only()));
            _bus.Publish(new ModifierCleared(C, ModifierId.Muddy));
            Assert.AreEqual(CarStatus.Serviced, Status.Resolve(Only()));
            _bus.Publish(new CarLeftStation(C, StationKind.Wash));
            Assert.AreEqual(CarStatus.Serviced, Status.Resolve(Only()), "still washed while being driven");
            _bus.Publish(new CarParked(C, 3));
            Assert.AreEqual(CarStatus.Parked, Status.Resolve(Only()));
        }

        [Test]
        public void ChargeThenPark_StatusReturnsToParked()
        {
            OpenCar(true, false);
            _bus.Publish(new CarParked(C, 7));
            _bus.Publish(new CarEnteredStation(C, StationKind.Charging));
            _bus.Publish(new ModifierProgressChanged(C, ModifierId.NeedsCharge, 0.4f));
            Assert.AreEqual(CarStatus.Servicing, Status.Resolve(Only()));
            _bus.Publish(new ModifierProgressChanged(C, ModifierId.NeedsCharge, 1f));
            _bus.Publish(new ModifierCleared(C, ModifierId.NeedsCharge));
            Assert.AreEqual(CarStatus.Serviced, Status.Resolve(Only()));
            _bus.Publish(new CarLeftSpot(C, 7));
            _bus.Publish(new CarLeftStation(C, StationKind.Charging));
            _bus.Publish(new CarParked(C, 2));
            Assert.AreEqual(CarStatus.Parked, Status.Resolve(Only()));
        }

        [Test]
        public void DrivenOutMidWash_IsNotWashed_AndParksAsParked()
        {
            OpenCar(false, true);
            _bus.Publish(new CarEnteredStation(C, StationKind.Wash));
            _bus.Publish(new ModifierProgressChanged(C, ModifierId.Muddy, 0.3f));
            _bus.Publish(new CarLeftStation(C, StationKind.Wash));
            Assert.AreEqual(CarStatus.Parked, Status.Resolve(Only()));
            _bus.Publish(new CarParked(C, 3));
            Assert.AreEqual(CarStatus.Parked, Status.Resolve(Only()));
        }

        [Test]
        public void PickupStillBeatsWashed_ThenParkingDoesNotHideIt()
        {
            OpenCar(false, true);
            _bus.Publish(new ModifierCleared(C, ModifierId.Muddy));
            _bus.Publish(new PickupRequested(T, C));
            Assert.AreEqual(CarStatus.PickUp, Status.Resolve(Only()));
            _bus.Publish(new CarParked(C, 3));
            Assert.AreEqual(CarStatus.PickUp, Status.Resolve(Only()));
        }
    }
}
