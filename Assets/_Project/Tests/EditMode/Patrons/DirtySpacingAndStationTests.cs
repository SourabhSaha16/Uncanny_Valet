using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class DirtySpacingAndStationTests
    {
        private static readonly ModifierSet Mud = ModifierSet.Of(ModifierId.Muddy);

        private static LotSnapshot LotWith(params StationKind[] kinds)
        {
            var slots = new StationSlot[kinds.Length];
            for (var i = 0; i < kinds.Length; i++) slots[i] = new StationSlot(kinds[i], StationSlotState.Free, default);
            return new LotSnapshot(null, slots, 0, 0, float.PositiveInfinity, 0f, null);
        }

        [Test]
        public void NoChargingBay_NoCarKeepsTheNeedsChargeModifier()
        {
            var catalog = ModifierDefaults.CreateCatalog();
            var lot = LotWith(StationKind.Wash);
            foreach (var archetype in new[] { CarArchetype.Ev, CarArchetype.MuddyEv })
            {
                var set = ModifierStationFilter.Serviceable(ArchetypeModifiers.Of(archetype), catalog, lot);
                Assert.IsFalse(set.Contains(ModifierId.NeedsCharge), archetype.ToString());
            }
        }

        [Test]
        public void MuddyEv_WithoutChargingBay_StillBringsMud()
        {
            var set = ModifierStationFilter.Serviceable(ArchetypeModifiers.Of(CarArchetype.MuddyEv),
                ModifierDefaults.CreateCatalog(), LotWith(StationKind.Wash));
            Assert.IsTrue(set.Contains(ModifierId.Muddy));
        }

        [Test]
        public void WithAChargingBay_NeedsChargeStays()
        {
            var set = ModifierStationFilter.Serviceable(ArchetypeModifiers.Of(CarArchetype.Ev),
                ModifierDefaults.CreateCatalog(), LotWith(StationKind.Charging));
            Assert.IsTrue(set.Contains(ModifierId.NeedsCharge));
        }

        [Test]
        public void EmptyLot_ServesNothing()
        {
            var set = ModifierStationFilter.Serviceable(ArchetypeModifiers.Of(CarArchetype.MuddyEv),
                ModifierDefaults.CreateCatalog(), LotSnapshot.Empty);
            Assert.IsTrue(set.IsEmpty);
        }

        [Test]
        public void TwoDirtyCarsInARow_SecondBecomesClean()
        {
            var spacing = new DirtySpacing(1);
            Assert.IsTrue(spacing.Apply(Mud).Contains(ModifierId.Muddy));
            Assert.IsFalse(spacing.Apply(Mud).Contains(ModifierId.Muddy));
        }

        [Test]
        public void OneCleanCarBetween_AllowsDirtyAgain()
        {
            var spacing = new DirtySpacing(1);
            spacing.Apply(Mud);
            spacing.Apply(ModifierSet.None);
            Assert.IsTrue(spacing.Apply(Mud).Contains(ModifierId.Muddy));
        }

        [Test]
        public void LongerGap_NeedsThatManyCleanCars()
        {
            var spacing = new DirtySpacing(2);
            spacing.Apply(Mud);
            Assert.IsFalse(spacing.Apply(Mud).Contains(ModifierId.Muddy));
            Assert.IsFalse(spacing.Apply(Mud).Contains(ModifierId.Muddy));
            Assert.IsTrue(spacing.Apply(Mud).Contains(ModifierId.Muddy), "two clean cars have passed");
        }

        [Test]
        public void AlwaysDirtyStream_NeverHasTwoDirtyInARow()
        {
            var spacing = new DirtySpacing(1);
            var lastDirty = false;
            for (var i = 0; i < 20; i++)
            {
                var dirty = spacing.Apply(Mud).Contains(ModifierId.Muddy);
                Assert.IsFalse(dirty && lastDirty, "arrival " + i);
                lastDirty = dirty;
            }
        }

        [Test]
        public void Reset_ForgetsThePreviousShiftsDirtyCar()
        {
            var spacing = new DirtySpacing(1);
            spacing.Apply(Mud);
            spacing.Reset();
            Assert.IsTrue(spacing.Apply(Mud).Contains(ModifierId.Muddy));
        }

        [Test]
        public void ZeroRequired_TurnsTheRuleOff()
        {
            var spacing = new DirtySpacing(0);
            spacing.Apply(Mud);
            Assert.IsTrue(spacing.Apply(Mud).Contains(ModifierId.Muddy));
        }

        [Test]
        public void OtherModifiers_PassUntouched()
        {
            var spacing = new DirtySpacing(1);
            spacing.Apply(Mud);
            var charge = ModifierSet.Of(ModifierId.NeedsCharge);
            Assert.AreEqual(charge, spacing.Apply(charge));
        }
    }
}
