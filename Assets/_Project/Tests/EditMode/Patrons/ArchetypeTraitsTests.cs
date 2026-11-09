using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class ArchetypeTraitsTests
    {
        [TestCase(CarArchetype.Plain, false, false)]
        [TestCase(CarArchetype.Dirty, false, true)]
        [TestCase(CarArchetype.Ev, true, false)]
        [TestCase(CarArchetype.DirtyEv, true, true)]
        public void ArchetypeMapsToTraits(CarArchetype archetype, bool ev, bool dirty)
        {
            ArchetypeTraits.Of(archetype, out var isEv, out var isDirty);
            Assert.AreEqual(ev, isEv);
            Assert.AreEqual(dirty, isDirty);
        }
    }
}
