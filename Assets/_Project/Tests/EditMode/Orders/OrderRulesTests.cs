using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Orders;

namespace UncannyValet.Tests.Orders
{
    public class OrderRulesTests
    {
        private static readonly CarId CarA = new CarId(1);
        private static readonly CarId CarB = new CarId(2);

        [Test]
        public void History_FindsVisitAtOrAfterTheMinuteOnly()
        {
            var history = new CarVisitHistory();
            history.RecordParked(CarA, 3, 10f);
            Assert.IsTrue(history.HasVisited(CarA, 3, 10f));
            Assert.IsFalse(history.HasVisited(CarA, 3, 10.5f), "visit was before the order");
            Assert.IsFalse(history.HasVisited(CarA, 4, 0f), "other spot");
            Assert.IsFalse(history.HasVisited(CarB, 3, 0f), "other car");
        }

        [Test]
        public void History_ClearForgetsOneCar()
        {
            var history = new CarVisitHistory();
            history.RecordParked(CarA, 3, 1f);
            history.RecordParked(CarB, 3, 1f);
            history.Clear(CarA);
            Assert.IsFalse(history.HasVisited(CarA, 3, 0f));
            Assert.IsTrue(history.HasVisited(CarB, 3, 0f));
            history.ClearAll();
            Assert.IsFalse(history.HasVisited(CarB, 3, 0f));
        }

        [Test]
        public void Judge_PassesAfterVisit_EvenIfCarLeftAgain()
        {
            var history = new CarVisitHistory();
            var judge = new OrderJudge(history);
            var order = new Order(new OrderId(1), CarA, 'A', 7, 20f);
            Assert.AreEqual(OrderOutcome.Failed, judge.Judge(order));
            history.RecordParked(CarA, 7, 25f);
            history.RecordParked(CarA, 2, 30f);
            Assert.AreEqual(OrderOutcome.Passed, judge.Judge(order));
        }

        [Test]
        public void Judge_IgnoresVisitBeforeOrder()
        {
            var history = new CarVisitHistory();
            history.RecordParked(CarA, 7, 5f);
            var order = new Order(new OrderId(1), CarA, 'A', 7, 20f);
            Assert.AreEqual(OrderOutcome.Failed, new OrderJudge(history).Judge(order));
        }

        [Test]
        public void Book_GivesIncreasingIdsAndLooksUpByCar()
        {
            var book = new OrderBook();
            var first = book.Open(CarA, 'A', 4, 1f);
            var second = book.Open(CarB, 'B', 5, 2f);
            Assert.AreEqual(1, first.Id.Value);
            Assert.AreEqual(2, second.Id.Value);
            Assert.IsTrue(book.TryGetByCar(CarB, out var found));
            Assert.AreEqual(5, found.TargetSpot);
            Assert.IsTrue(book.IsSpotTargeted(4));
            Assert.IsTrue(book.Remove(CarA));
            Assert.IsFalse(book.IsSpotTargeted(4));
            Assert.AreEqual(1, book.Count);
            book.Clear();
            Assert.AreEqual(3, book.Open(CarA, 'A', 4, 3f).Id.Value, "ids keep counting");
        }

        [Test]
        public void ParkedCars_OnlyEligibleCarsAreCollected()
        {
            var parked = new ParkedCars();
            var book = new OrderBook();
            var cars = new List<ParkedCar>();
            parked.SetLetter(CarA, 'A'); parked.Park(CarA, 1);
            parked.SetLetter(CarB, 'B');
            parked.Park(new CarId(3), 3);
            parked.CollectEligible(book, cars);
            Assert.AreEqual(1, cars.Count, "car B is not parked and car 3 has no letter");
            Assert.AreEqual('A', cars[0].Letter);

            book.Open(CarA, 'A', 2, 0f);
            cars.Clear();
            parked.CollectEligible(book, cars);
            Assert.AreEqual(0, cars.Count, "car A has an open order");
        }

        [Test]
        public void ParkedCars_PickupRequestedAndLeftCarsAreSkipped()
        {
            var parked = new ParkedCars();
            var cars = new List<ParkedCar>();
            parked.SetLetter(CarA, 'A'); parked.Park(CarA, 1); parked.RequestPickup(CarA);
            parked.SetLetter(CarB, 'B'); parked.Park(CarB, 2); parked.LeaveSpot(CarB, 2);
            parked.CollectEligible(new OrderBook(), cars);
            Assert.AreEqual(0, cars.Count);
        }

        [Test]
        public void ParkedCars_LeavingAnotherSpotKeepsTheCurrentOne()
        {
            var parked = new ParkedCars();
            var cars = new List<ParkedCar>();
            parked.SetLetter(CarA, 'A'); parked.Park(CarA, 1); parked.LeaveSpot(CarA, 9);
            parked.CollectEligible(new OrderBook(), cars);
            Assert.AreEqual(1, cars.Count);
        }

        [Test]
        public void Selector_UsesScriptedCarAndSpot()
        {
            var selector = new OrderSelector(new ScriptedOrderRandom().Picks(1, 2));
            var cars = new List<ParkedCar> { new ParkedCar(CarA, 'A', 1), new ParkedCar(CarB, 'B', 2) };
            var spots = new List<int> { 5, 6, 7 };
            Assert.IsTrue(selector.TryPick(cars, spots, out var pick));
            Assert.AreEqual(CarB, pick.Car.Car);
            Assert.AreEqual(7, pick.TargetSpot);
        }

        [Test]
        public void Selector_SkipsWithoutCarsOrFreeSpots()
        {
            var selector = new OrderSelector(new ScriptedOrderRandom());
            var cars = new List<ParkedCar> { new ParkedCar(CarA, 'A', 1) };
            Assert.IsFalse(selector.TryPick(new List<ParkedCar>(), new List<int> { 5 }, out _));
            Assert.IsFalse(selector.TryPick(cars, new List<int>(), out _));
        }

        [Test]
        public void Scheduler_FiresOncePerGapAndRearms()
        {
            var scheduler = new OrderScheduler(new ScriptedOrderRandom().Gaps(10f, 15f));
            Assert.IsFalse(scheduler.IsArmed);
            Assert.IsFalse(scheduler.TryConsume(100f, 5f, 20f), "disarmed never fires");
            scheduler.Arm(0f, 5f, 20f);
            Assert.IsFalse(scheduler.TryConsume(9f, 5f, 20f));
            Assert.IsTrue(scheduler.TryConsume(10f, 5f, 20f));
            Assert.IsFalse(scheduler.TryConsume(24f, 5f, 20f), "next gap is 15 after minute 10");
            Assert.IsTrue(scheduler.TryConsume(25f, 5f, 20f));
            scheduler.Disarm();
            Assert.IsFalse(scheduler.TryConsume(500f, 5f, 20f));
        }
    }
}
