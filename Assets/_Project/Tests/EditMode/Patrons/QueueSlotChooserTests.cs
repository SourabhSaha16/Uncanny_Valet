using NUnit.Framework;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class QueueSlotChooserTests
    {
        private const int Usable = 3;
        private static readonly bool[] Free = { false, false, false };

        private readonly QueueSlotChooser _chooser = new QueueSlotChooser();

        [Test]
        public void EmptyQueue_TakesSlotZero()
        {
            Assert.AreEqual(0, _chooser.Choose(Free, Free, Usable));
        }

        [Test]
        public void OccupiedSlotZero_TakesSlotOne()
        {
            Assert.AreEqual(1, _chooser.Choose(new[] { true, false, false }, Free, Usable));
        }

        [Test]
        public void SlotFreedAfterAssignment_MovesUp()
        {
            Assert.AreEqual(1, _chooser.Choose(new[] { true, false, false }, Free, Usable));
            Assert.AreEqual(0, _chooser.Choose(Free, Free, Usable));
        }

        [Test]
        public void SlotReservedByACarAhead_IsSkipped()
        {
            Assert.AreEqual(1, _chooser.Choose(Free, new[] { true, false, false }, Usable));
        }

        [Test]
        public void OccupiedSlotNearTheRoad_CutsOffTheEmptySlotsBehindIt()
        {
            Assert.AreEqual(2, _chooser.Choose(new[] { false, true, false }, Free, Usable), "a car cannot drive through slot 1 to slot 0");
        }

        [Test]
        public void SlotsAheadAllOccupied_TakesTheLastFreeSlot()
        {
            Assert.AreEqual(2, _chooser.Choose(new[] { true, true, false }, Free, Usable));
        }

        [Test]
        public void EverySlotTaken_GivesNoSlot()
        {
            Assert.AreEqual(ArrivalGate.NoSlot, _chooser.Choose(new[] { true, false, false }, new[] { false, true, true }, Usable));
        }

        [Test]
        public void OnlyTheUsableSlotsCount()
        {
            Assert.AreEqual(ArrivalGate.NoSlot, _chooser.Choose(new[] { true, true, false }, Free, 2));
        }
    }
}
