using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Lot;

namespace UncannyValet.Tests.Lot
{
    public class SmashPlanTests
    {
        private readonly List<SmashAssignment> _result = new List<SmashAssignment>();

        private static SmashCandidate Car(int id, bool ev = false, bool dirty = false) => new SmashCandidate(new CarId(id), ev, dirty);

        private int Assign(SmashCandidate[] cars, int[] regular, int[] charging)
            => SmashPlan.Assign(cars, regular, charging, _result);

        [Test]
        public void PlainCars_TakeRegularBaysInOrder()
        {
            Assert.AreEqual(2, Assign(new[] { Car(1), Car(2) }, new[] { 3, 4, 5 }, new[] { 7 }));
            Assert.AreEqual(3, _result[0].SpotId);
            Assert.AreEqual(4, _result[1].SpotId);
            Assert.IsFalse(_result[0].Charged);
        }

        [Test]
        public void PlainCars_NeverTakeAChargingBay()
        {
            Assert.AreEqual(1, Assign(new[] { Car(1), Car(2) }, new[] { 3 }, new[] { 7 }));
            Assert.AreEqual(1, _result.Count);
        }

        [Test]
        public void Ev_TakesAChargingBayAndIsCharged()
        {
            Assign(new[] { Car(1, ev: true) }, new[] { 3 }, new[] { 7 });
            Assert.AreEqual(7, _result[0].SpotId);
            Assert.IsTrue(_result[0].Charged);
        }

        [Test]
        public void Ev_WithNoChargingBayLeft_FallsBackToARegularBayUncharged()
        {
            Assert.AreEqual(2, Assign(new[] { Car(1, ev: true), Car(2, ev: true) }, new[] { 3 }, new[] { 7 }));
            Assert.AreEqual(7, _result[0].SpotId);
            Assert.AreEqual(3, _result[1].SpotId);
            Assert.IsFalse(_result[1].Charged);
        }

        [Test]
        public void DirtyCars_AreWashed_PlainOnesAreNot()
        {
            Assign(new[] { Car(1, dirty: true), Car(2) }, new[] { 3, 4 }, new int[0]);
            Assert.IsTrue(_result[0].Washed);
            Assert.IsFalse(_result[1].Washed);
        }

        [Test]
        public void DirtyEv_IsChargedAndWashed()
        {
            Assign(new[] { Car(1, ev: true, dirty: true) }, new[] { 3 }, new[] { 7 });
            Assert.IsTrue(_result[0].Charged);
            Assert.IsTrue(_result[0].Washed);
        }

        [Test]
        public void CarsThatDoNotFit_AreNotAssigned_AndEarlierCarsKeepTheirBays()
        {
            Assert.AreEqual(1, Assign(new[] { Car(1), Car(2), Car(3) }, new[] { 3 }, new int[0]));
            Assert.AreEqual(0, _result[0].Candidate);
        }

        [Test]
        public void NoseIn_BaysAboveTheAisleFaceUp_BaysBelowFaceDown()
        {
            Assert.AreEqual(0f, SmashPlan.NoseInDegrees(5f, 0f));
            Assert.AreEqual(180f, SmashPlan.NoseInDegrees(-5f, 0f));
        }

        [Test]
        public void NoFreeBays_AssignsNothing()
        {
            Assert.AreEqual(0, Assign(new[] { Car(1, ev: true) }, new int[0], new int[0]));
        }

        [Test]
        public void ResultIsClearedOnEveryCall()
        {
            Assign(new[] { Car(1) }, new[] { 3 }, new int[0]);
            Assign(new SmashCandidate[0], new[] { 3 }, new int[0]);
            Assert.AreEqual(0, _result.Count);
        }
    }

    public class SmashRosterTests
    {
        private readonly SmashRoster _roster = new SmashRoster();
        private readonly List<SmashCandidate> _list = new List<SmashCandidate>();
        private readonly CarId _a = new CarId(1);
        private readonly CarId _b = new CarId(2);

        [Test]
        public void ArrivedCars_AreListedInArrivalOrderWithTheirTraits()
        {
            _roster.Traits(_b, true, false);
            _roster.Traits(_a, false, true);
            _roster.Arrived(_a);
            _roster.Arrived(_b);
            _roster.Arrived(_a);
            _roster.CopyTo(_list);
            Assert.AreEqual(2, _list.Count);
            Assert.AreEqual(_a, _list[0].Car);
            Assert.IsTrue(_list[0].IsDirty);
            Assert.IsTrue(_list[1].IsEv);
        }

        [Test]
        public void ParkedOrRequestedCar_LeavesTheWaitingList()
        {
            _roster.Arrived(_a);
            _roster.Arrived(_b);
            _roster.Remove(_a);
            _roster.CopyTo(_list);
            Assert.AreEqual(1, _list.Count);
            Assert.AreEqual(_b, _list[0].Car);
        }

        [Test]
        public void ClosedTicket_ForgetsTheCarAndItsTraits()
        {
            _roster.Traits(_a, true, true);
            _roster.Arrived(_a);
            _roster.Closed(_a);
            _roster.Arrived(_a);
            _roster.CopyTo(_list);
            Assert.IsFalse(_list[0].IsEv);
        }

        [Test]
        public void Clear_EmptiesTheRoster()
        {
            _roster.Arrived(_a);
            _roster.Clear();
            Assert.AreEqual(0, _roster.Count);
        }
    }
}
