using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    public class DilemmaRuleTests
    {
        private readonly DilemmaRuleSettings _settings = new DilemmaRuleSettings();
        private IDilemmaRule[] _rules;

        [SetUp]
        public void SetUp() => _rules = DilemmaRules.CreateAll(_settings);

        private static SnapshotBuilderForTests Lot(StationSlotState wash, StationSlotState bayA, StationSlotState bayB)
        {
            var b = new SnapshotBuilderForTests();
            b.Slot(StationKind.Wash, wash);
            b.Slot(StationKind.Charging, bayA);
            b.Slot(StationKind.Charging, bayB);
            return b;
        }

        // Which rules fire (trigger holds) on this lot.
        private List<DilemmaKind> Fired(SnapshotBuilderForTests b)
        {
            var lot = b.Build();
            var fired = new List<DilemmaKind>();
            foreach (var rule in _rules) if (rule.Evaluate(lot).HasValue) fired.Add(rule.Kind);
            return fired;
        }

        private DilemmaBoost Boost(DilemmaKind kind, SnapshotBuilderForTests b)
            => DilemmaRules.Find(_rules, kind).Evaluate(b.Build()).Value;

        [Test]
        public void AllSevenRulesExistWithDistinctKinds()
        {
            Assert.AreEqual(7, _rules.Length);
            var kinds = new HashSet<DilemmaKind>();
            foreach (var rule in _rules) kinds.Add(rule.Kind);
            Assert.AreEqual(7, kinds.Count);
        }

        [Test]
        public void NothingFiresOnAnIdleLotWithRoom()
        {
            Assert.IsEmpty(Fired(Lot(StationSlotState.Free, StationSlotState.Free, StationSlotState.Free)));
        }

        [Test]
        public void WashBlockedFiresOnlyOnABlockedWashSlot()
        {
            var b = Lot(StationSlotState.Blocked, StationSlotState.Free, StationSlotState.Free);
            CollectionAssert.AreEqual(new[] { DilemmaKind.WashBlocked }, Fired(b));
            var boost = Boost(DilemmaKind.WashBlocked, b);
            Assert.AreEqual(CarArchetype.Muddy, boost.Archetype);
            Assert.AreEqual(3f, boost.Multiplier);
            Assert.IsTrue(boost.Solvable);
        }

        [Test]
        public void WashBusyFiresWhenTheWashIsWorkingAndNothingIsQueued()
        {
            var b = Lot(StationSlotState.Working, StationSlotState.Free, StationSlotState.Free);
            CollectionAssert.AreEqual(new[] { DilemmaKind.WashBusy }, Fired(b));
            Assert.AreEqual(1.5f, Boost(DilemmaKind.WashBusy, b).Multiplier);
            Assert.AreEqual(CarArchetype.Muddy, Boost(DilemmaKind.WashBusy, b).Archetype);
        }

        [Test]
        public void WashBusyStaysQuietWhenAMuddyCarIsAlreadyQueued()
        {
            var b = Lot(StationSlotState.Working, StationSlotState.Free, StationSlotState.Free);
            b.Car(CarLotState.WaitingToPark, muddy: true, awaitingPark: true);
            Assert.IsEmpty(Fired(b));
        }

        [Test]
        public void BaysBlockedNeedsEveryBayBlocked()
        {
            var both = Lot(StationSlotState.Free, StationSlotState.Blocked, StationSlotState.Blocked);
            CollectionAssert.AreEqual(new[] { DilemmaKind.BaysBlocked }, Fired(both));
            Assert.AreEqual(CarArchetype.Ev, Boost(DilemmaKind.BaysBlocked, both).Archetype);
            Assert.AreEqual(3f, Boost(DilemmaKind.BaysBlocked, both).Multiplier);
            var one = Lot(StationSlotState.Free, StationSlotState.Blocked, StationSlotState.Working);
            Assert.IsEmpty(Fired(one));
        }

        [Test]
        public void BaySquattedFiresOnASquattedBay()
        {
            var b = Lot(StationSlotState.Free, StationSlotState.Squatted, StationSlotState.Free);
            CollectionAssert.AreEqual(new[] { DilemmaKind.BaySquatted }, Fired(b));
            Assert.AreEqual(2.5f, Boost(DilemmaKind.BaySquatted, b).Multiplier);
        }

        [Test]
        public void LastBayNeedsOneFreeAndTheOtherWorking()
        {
            var last = Lot(StationSlotState.Free, StationSlotState.Free, StationSlotState.Working);
            CollectionAssert.AreEqual(new[] { DilemmaKind.LastBay }, Fired(last));
            Assert.AreEqual(1.5f, Boost(DilemmaKind.LastBay, last).Multiplier);
            Assert.IsEmpty(Fired(Lot(StationSlotState.Free, StationSlotState.Free, StationSlotState.Free)));
            Assert.IsEmpty(Fired(Lot(StationSlotState.Free, StationSlotState.Working, StationSlotState.Working)));
            Assert.IsEmpty(Fired(Lot(StationSlotState.Free, StationSlotState.Free, StationSlotState.Blocked)));
        }

        [Test]
        public void LastBayWithASingleBayNeedsTheWashWorking()
        {
            var b = new SnapshotBuilderForTests();
            b.Slot(StationKind.Wash, StationSlotState.Working);
            b.Slot(StationKind.Charging, StationSlotState.Free);
            CollectionAssert.AreEquivalent(new[] { DilemmaKind.LastBay, DilemmaKind.WashBusy }, Fired(b));
            var idleWash = new SnapshotBuilderForTests();
            idleWash.Slot(StationKind.Wash, StationSlotState.Free);
            idleWash.Slot(StationKind.Charging, StationSlotState.Free);
            Assert.IsEmpty(Fired(idleWash));
            var busyBay = new SnapshotBuilderForTests();
            busyBay.Slot(StationKind.Wash, StationSlotState.Working);
            busyBay.Slot(StationKind.Charging, StationSlotState.Working);
            CollectionAssert.AreEqual(new[] { DilemmaKind.WashBusy }, Fired(busyBay));
        }

        [Test]
        public void LotTightFiresAtOneFreeSpotAndAsksForAnEarlyReturn()
        {
            var b = Lot(StationSlotState.Free, StationSlotState.Free, StationSlotState.Free);
            b.FreeSpots = 1;
            b.TotalSpots = 6;
            CollectionAssert.AreEqual(new[] { DilemmaKind.LotTight }, Fired(b));
            var boost = Boost(DilemmaKind.LotTight, b);
            Assert.AreEqual(CarArchetype.Plain, boost.Archetype);
            Assert.AreEqual(1.5f, boost.Multiplier);
            Assert.IsTrue(boost.BringReturnForward);
            b.FreeSpots = 2;
            Assert.IsEmpty(Fired(b));
        }

        [Test]
        public void ChainNeedsBothStationsBlocked()
        {
            var b = Lot(StationSlotState.Blocked, StationSlotState.Blocked, StationSlotState.Blocked);
            CollectionAssert.AreEquivalent(
                new[] { DilemmaKind.WashBlocked, DilemmaKind.BaysBlocked, DilemmaKind.Chain }, Fired(b));
            Assert.AreEqual(CarArchetype.MuddyEv, Boost(DilemmaKind.Chain, b).Archetype);
            Assert.AreEqual(2f, Boost(DilemmaKind.Chain, b).Multiplier);
            var half = Lot(StationSlotState.Blocked, StationSlotState.Blocked, StationSlotState.Working);
            CollectionAssert.AreEqual(new[] { DilemmaKind.WashBlocked }, Fired(half));
        }

        [Test]
        public void EveryRuleReportsUnsolvableWithNoFreeSpotAndNoPickupDue()
        {
            var b = Lot(StationSlotState.Blocked, StationSlotState.Blocked, StationSlotState.Blocked);
            b.FreeSpots = 0;
            b.TotalSpots = 9;
            var lot = b.Build();
            var checkedRules = 0;
            foreach (var rule in _rules)
            {
                var boost = rule.Evaluate(lot);
                if (!boost.HasValue) continue;
                Assert.IsFalse(boost.Value.Solvable, rule.Kind + " must be unsolvable");
                checkedRules++;
            }
            Assert.AreEqual(4, checkedRules, "wash blocked, bays blocked, chain and lot tight all trigger here");
        }

        [Test]
        public void ADueSoonPickupMakesAFullLotSolvable()
        {
            var b = Lot(StationSlotState.Blocked, StationSlotState.Free, StationSlotState.Free);
            b.FreeSpots = 0;
            b.TotalSpots = 9;
            b.NextPickup = 6f;
            Assert.IsTrue(Boost(DilemmaKind.WashBlocked, b).Solvable);
            b.NextPickup = 12f;
            Assert.IsFalse(Boost(DilemmaKind.WashBlocked, b).Solvable);
        }

        [Test]
        public void ARuleWithoutThatStationFiresNothing()
        {
            var b = new SnapshotBuilderForTests();
            Assert.IsEmpty(Fired(b), "no stations unlocked, plenty of room");
        }
    }
}
