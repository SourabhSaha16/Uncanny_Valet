using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;
using UnityEngine;

namespace UncannyValet.Tests.Patrons
{
    /// <summary>The pure queue rules of the valet pickup spot (issue 127): moving up, the exit gate and slot lookup.</summary>
    public class QueueAdvanceRuleTests
    {
        private static readonly CarId None = CarId.None;
        private static readonly CarId A = new CarId(1);
        private static readonly CarId B = new CarId(2);
        private static readonly CarId C = new CarId(3);
        private static readonly CarId D = new CarId(4);

        private static List<QueueMove> Moves(CarId[] standing, bool[] occupied)
        {
            var into = new List<QueueMove>();
            QueueAdvanceRule.Moves(standing, occupied, into);
            return into;
        }

        [Test]
        public void FullQueue_NobodyMoves()
        {
            Assert.AreEqual(0, Moves(new[] { A, B, C, D }, new[] { true, true, true, true }).Count);
        }

        [Test]
        public void PickupSpotFreed_TheCarBehindDrivesIntoIt()
        {
            var moves = Moves(new[] { None, B, C, D }, new[] { false, true, true, true });
            Assert.AreEqual(1, moves.Count);
            Assert.AreEqual(B, moves[0].Car);
            Assert.AreEqual(0, moves[0].ToSlot);
        }

        [Test]
        public void OnlyOneCarClaimsAFreeSlot_TheRestWaitUntilTheirSlotClears()
        {
            var moves = Moves(new[] { None, B, C, D }, new[] { false, true, true, true });
            Assert.AreEqual(1, moves.Count, "slot 1 is still physically taken by the car that is leaving it");
        }

        [Test]
        public void EveryCarMovesUpOnceItsSlotAheadIsClear()
        {
            var moves = Moves(new[] { None, None, C, D }, new[] { false, false, true, true });
            Assert.AreEqual(1, moves.Count);
            Assert.AreEqual(C, moves[0].Car);
            Assert.AreEqual(1, moves[0].ToSlot);
        }

        [Test]
        public void TwoCarsMoveInTheSameStep_WhenTheirTargetsAreBothClear()
        {
            var moves = Moves(new[] { None, B, None, D }, new[] { false, true, false, true });
            Assert.AreEqual(2, moves.Count);
            Assert.AreEqual(0, moves[0].ToSlot);
            Assert.AreEqual(2, moves[1].ToSlot);
        }

        [Test]
        public void ASlotAlreadyBeingDrivenInto_IsNotClaimedAgain()
        {
            var moves = Moves(new[] { None, B, C, D }, new[] { true, true, true, true });
            Assert.AreEqual(0, moves.Count, "occupied includes slots a car is already driving into");
        }

        [Test]
        public void TheFreedTailSlotAcceptsTheNextArrival_AfterTheQueueClosedUp()
        {
            var standing = new[] { B, C, D, None };
            var occupied = new[] { true, true, true, false };
            Assert.AreEqual(0, Moves(standing, occupied).Count, "nothing to advance and the tail slot is free");
        }

        [Test]
        public void SlotAt_FindsTheNearestSlotWithinTheTolerance()
        {
            var slots = new[] { new Vector2(0f, 0f), new Vector2(0f, -4.65f), new Vector2(0f, -9.3f) };
            Assert.AreEqual(1, QueueSlotLocator.SlotAt(slots, new Vector2(0.2f, -4f), 1.5f));
            Assert.AreEqual(QueueSlotLocator.None, QueueSlotLocator.SlotAt(slots, new Vector2(0f, -2.3f), 1.5f), "between slots: in none");
            Assert.AreEqual(0, QueueSlotLocator.SlotAt(slots, new Vector2(0.9f, 0.5f), 1.5f));
        }

        [Test]
        public void TheValetAtThePickupCar_ReleasesThePatronOnce()
        {
            var gate = new PatronExitGate();
            Assert.IsTrue(gate.TryReleaseByApproach(A, QueueAdvanceRule.PickupSlot, 3f, 4f));
            Assert.IsFalse(gate.TryReleaseByApproach(A, QueueAdvanceRule.PickupSlot, 1f, 4f), "only once per car");
            Assert.IsTrue(gate.IsReleased(A));
        }

        [Test]
        public void TheValetFarFromThePickupCar_DoesNotReleaseThePatron()
        {
            var gate = new PatronExitGate();
            Assert.IsFalse(gate.TryReleaseByApproach(A, QueueAdvanceRule.PickupSlot, 4.5f, 4f));
            Assert.IsFalse(gate.IsReleased(A));
        }

        [Test]
        public void TheValetAtACarFurtherBack_DoesNotReleaseThatPatron()
        {
            var gate = new PatronExitGate();
            Assert.IsFalse(gate.TryReleaseByApproach(B, 1, 0.5f, 4f));
            Assert.IsFalse(gate.TryReleaseByApproach(C, 3, 0.5f, 4f));
        }

        [Test]
        public void ACarTakenFromTheQueue_ReleasesItsPatronOnce_AndAForgottenCarCanBeReleasedAgain()
        {
            var gate = new PatronExitGate();
            Assert.IsTrue(gate.TryReleaseByTakeover(B));
            Assert.IsFalse(gate.TryReleaseByTakeover(B));
            gate.Forget(B);
            Assert.IsFalse(gate.IsReleased(B));
        }
    }
}
