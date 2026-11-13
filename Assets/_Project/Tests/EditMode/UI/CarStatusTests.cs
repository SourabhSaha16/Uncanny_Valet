using System;
using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.UI;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Mood;
using UncannyValet.UI.Status;

namespace UncannyValet.Tests.UI
{
    public class CarStatusTests
    {
        private static readonly CarStatusResolver Resolver = CarStatusResolver.CreateDefault(new EmotionRule(0.6f, 0.85f));

        private static TicketRow Parked() => new TicketRow { Placed = true, State = TicketRowState.Parked };

        private static TicketRow Pickup(bool expired = false)
            => new TicketRow { Placed = true, State = TicketRowState.PickupRequested, Expired = expired };

        [Test]
        public void PlainParkedCar_IsParked() => Assert.AreEqual(CarStatus.Parked, Resolver.Resolve(Parked()));

        [Test]
        public void MuddyCarInTheWashSlot_IsServicing_ThenServiced_WithTheWordsOfTheModifier()
        {
            var row = Parked().Give(false, true);
            row.HasStation = true;
            row.Station = StationKind.Wash;
            Assert.AreEqual(CarStatus.Servicing, Resolver.Resolve(row));
            Assert.AreEqual("Washing", CarStatusTable.LabelFitting(CarStatus.Servicing, row, 200));
            row.Clear(ModifierId.Muddy);
            Assert.AreEqual(CarStatus.Serviced, Resolver.Resolve(row));
            Assert.AreEqual("Washed", CarStatusTable.LabelFitting(CarStatus.Serviced, row, 200));
        }

        [Test]
        public void CarNeedingAChargeOnAChargerIsServicing_ThenServiced_WithTheChargeWords()
        {
            var row = Parked().Give(true, false);
            row.HasStation = true;
            row.Station = StationKind.Charging;
            Assert.AreEqual(CarStatus.Servicing, Resolver.Resolve(row));
            Assert.AreEqual("Charging", CarStatusTable.LabelFitting(CarStatus.Servicing, row, 200));
            row.Clear(ModifierId.NeedsCharge);
            Assert.AreEqual(CarStatus.Serviced, Resolver.Resolve(row));
            Assert.AreEqual("Charged", CarStatusTable.LabelFitting(CarStatus.Serviced, row, 200));
        }

        [Test]
        public void ChargedCarThatLeftTheCharger_StaysServiced()
        {
            var row = Parked().Give(true, false);
            row.Clear(ModifierId.NeedsCharge);
            Assert.AreEqual(CarStatus.Serviced, Resolver.Resolve(row));
        }

        [Test]
        public void MuddyCarParkedInASpot_IsJustParked()
        {
            var row = Parked().Give(false, true);
            Assert.AreEqual(CarStatus.Parked, Resolver.Resolve(row));
        }

        [Test]
        public void PickupBeatsEverythingBelow_AndAngerBeatsPickup()
        {
            var row = Pickup().Give(true, true);
            row.Clear(ModifierId.Muddy);
            row.Clear(ModifierId.NeedsCharge);
            Assert.AreEqual(CarStatus.PickUp, Resolver.Resolve(row));
            row.Elapsed01 = 0.9f;
            Assert.AreEqual(CarStatus.CustAngry, Resolver.Resolve(row));
            row.Expired = true;
            Assert.AreEqual(CarStatus.WillNotTip, Resolver.Resolve(row));
        }

        [Test]
        public void InProgressBeatsDone()
        {
            var row = Parked().Give(true, true);
            row.Clear(ModifierId.Muddy);
            row.HasStation = true;
            row.Station = StationKind.Charging;
            Assert.AreEqual(CarStatus.Servicing, Resolver.Resolve(row));
        }

        [Test]
        public void EveryStatusHasALabelAndAColour()
        {
            Assert.AreEqual(Enum.GetValues(typeof(CarStatus)).Length, CarStatusTable.Count);
            foreach (CarStatus status in Enum.GetValues(typeof(CarStatus)))
                Assert.IsNotEmpty(CarStatusTable.Label(status), status.ToString());
        }

        [Test]
        public void Colours_FollowTheStyleGuide()
        {
            Assert.AreEqual(PixelPalette.Amber, CarStatusTable.ColorOf(CarStatus.PickUp));
            Assert.AreEqual(PixelPalette.OrangeRed, CarStatusTable.ColorOf(CarStatus.CustAngry));
            Assert.AreEqual(PixelPalette.Green, CarStatusTable.ColorOf(CarStatus.Serviced));
            Assert.AreEqual(PixelPalette.Cyan, CarStatusTable.ColorOf(CarStatus.Servicing));
            Assert.AreEqual(PixelPalette.Cyan, CarStatusTable.ColorOf(CarStatus.Parked));
        }

        [Test]
        public void LongLabelIsAbbreviatedOnlyWhenItDoesNotFit()
        {
            Assert.AreEqual("Cust. Angry", CarStatusTable.LabelFitting(CarStatus.CustAngry, 200));
            Assert.AreEqual("Angry", CarStatusTable.LabelFitting(CarStatus.CustAngry, 56));
            Assert.AreEqual("Won't tip", CarStatusTable.LabelFitting(CarStatus.WillNotTip, 56));
            Assert.AreEqual("No tip", CarStatusTable.LabelFitting(CarStatus.WillNotTip, 40));
            Assert.AreEqual("Pick Up", CarStatusTable.LabelFitting(CarStatus.PickUp, 56));
        }

        // How a new status is added: Overheating beats everything just by sitting first in the ordered list.
        private sealed class OverheatingRule : ICarStatusRule
        {
            public CarStatus Status => (CarStatus)100;

            public bool Applies(TicketRow row) => row.Spot == 99;
        }

        [Test]
        public void ANewRuleJoinsTheOrderedList()
        {
            var rules = new List<ICarStatusRule> { new OverheatingRule(), new PickUpStatusRule() };
            var resolver = new CarStatusResolver(rules, CarStatus.Parked);
            var row = Pickup();
            Assert.AreEqual(CarStatus.PickUp, resolver.Resolve(row));
            row.Spot = 99;
            Assert.AreEqual((CarStatus)100, resolver.Resolve(row));
        }

        [Test]
        public void BoardTracksTheStationFromEvents()
        {
            var bus = new EventBus();
            var board = new TicketBoard(bus, ModifierTestKit.Catalog());
            var car = new CarId(3);
            bus.Publish(new TicketOpened(new TicketId(1), car, 60f));
            bus.Publish(new CarEnteredStation(car, StationKind.Wash));
            var rows = new List<TicketRow>();
            board.CollectRows(rows);
            Assert.IsTrue(rows[0].InStation(StationKind.Wash));
            bus.Publish(new CarLeftStation(car, StationKind.Wash));
            Assert.IsFalse(rows[0].InStation(StationKind.Wash));
            board.Dispose();
        }
    }
}
