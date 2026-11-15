using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Pedestrians;

namespace UncannyValet.Tests.Pedestrians
{
    public class PatronPoolTests
    {
        private sealed class Person { }

        private static List<Person> People(int count)
        {
            var list = new List<Person>();
            for (var i = 0; i < count; i++) list.Add(new Person());
            return list;
        }

        [Test]
        public void SizeIsSpotsPlusQueueSlots()
            => Assert.AreEqual(14, PatronPoolSizing.Compute(6, 8, 6));

        [Test]
        public void FloorWinsForATinyLot()
            => Assert.AreEqual(6, PatronPoolSizing.Compute(6, 2, 1));

        [Test]
        public void NegativeCountsAreIgnored()
            => Assert.AreEqual(4, PatronPoolSizing.Compute(4, -3, -1));

        [Test]
        public void EveryOpenTicketGetsAPersonWhenPoolIsSizedByTheRule()
        {
            const int spots = 9, queue = 5;
            var pool = new PatronPool<Person>(People(PatronPoolSizing.Compute(6, spots, queue)));
            var pending = new PendingStepOuts();
            for (var ticket = 1; ticket <= spots + queue; ticket++)
                if (!pool.TryRent(out _)) pending.Add(new CarId(ticket));
            Assert.AreEqual(0, pending.Count);
        }

        [Test]
        public void ExtraTicketWaitsOnceAndIsServedWhenAPersonReturns()
        {
            var members = People(2);
            var pool = new PatronPool<Person>(members);
            var pending = new PendingStepOuts();
            Assert.IsTrue(pool.TryRent(out var first));
            Assert.IsTrue(pool.TryRent(out _));
            Assert.IsFalse(pool.TryRent(out _));
            Assert.IsTrue(pending.Add(new CarId(3)));
            Assert.IsFalse(pending.Add(new CarId(3)), "a second failure for the same car is not a new report");
            pool.Return(first);
            Assert.AreEqual(1, pool.FreeCount);
            Assert.AreEqual(new CarId(3), pending.CarAt(0));
        }

        [Test]
        public void PoolOnlyHandsOutItsOwnMembersNeverTheAmbientCrowd()
        {
            var ambient = People(8);
            var patrons = People(3);
            var pool = new PatronPool<Person>(patrons);
            while (pool.TryRent(out var rented))
            {
                Assert.IsTrue(patrons.Contains(rented));
                Assert.IsFalse(ambient.Contains(rented));
            }
        }

        [Test]
        public void SummonedFlagAndClearWork()
        {
            var pending = new PendingStepOuts();
            pending.Add(new CarId(1));
            pending.MarkSummoned(new CarId(1));
            Assert.IsTrue(pending.SummonedAt(0));
            pending.Remove(new CarId(1));
            Assert.AreEqual(0, pending.Count);
            pending.Add(new CarId(2));
            pending.Clear();
            Assert.AreEqual(0, pending.Count);
        }
    }
}
