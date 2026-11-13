using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Tests.Patrons
{
    public class ArchetypeModifiersTests
    {
        [TestCase(CarArchetype.Plain, false, false)]
        [TestCase(CarArchetype.Muddy, false, true)]
        [TestCase(CarArchetype.Ev, true, false)]
        [TestCase(CarArchetype.MuddyEv, true, true)]
        public void ArchetypeMapsToModifiers(CarArchetype archetype, bool needsCharge, bool muddy)
        {
            var set = ArchetypeModifiers.Of(archetype);
            Assert.AreEqual(needsCharge, set.Contains(ModifierId.NeedsCharge));
            Assert.AreEqual(muddy, set.Contains(ModifierId.Muddy));
            Assert.AreEqual((needsCharge ? 1 : 0) + (muddy ? 1 : 0), set.Count);
        }
    }
}
