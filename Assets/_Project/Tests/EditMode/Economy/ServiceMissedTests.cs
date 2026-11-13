using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Economy;

namespace UncannyValet.Tests.Economy
{
    /// <summary>The shared service-missed rule: any open service modifier halves the final amount once, favour untouched.</summary>
    public class ServiceMissedTests
    {
        private const ModifierId FakeService = (ModifierId)3;
        private static readonly ModifierSet Muddy = ModifierSet.Of(ModifierId.Muddy);
        private static readonly ModifierSet Charge = ModifierSet.Of(ModifierId.NeedsCharge);

        private BlessingCalculator _calc;

        [SetUp]
        public void SetUp() => _calc = new BlessingCalculator(ScriptableObject.CreateInstance<BlessingConfig>(), null, ModifierTestKit.Catalog());

        private BlessingBreakdown Explain(int baseBlessing, ModifierSet open = default, ModifierSet cleared = default,
            float damage = 0f, int favour = 1)
            => _calc.Explain(new BlessingInput(baseBlessing, 5f, false, clearedModifiers: cleared, openModifiers: open,
                damage01: damage, favourMultiplier: favour));

        private static int Half(int total) => (total + 1) / 2;

        [Test]
        public void WashedMuddyCar_PaysFull_AndIsNotFlagged()
        {
            var b = Explain(10, cleared: Muddy);
            Assert.IsFalse(b.ServiceMissed);
            Assert.AreEqual(Explain(10).Total + b.ModifierBonus, b.Total);
        }

        [Test]
        public void UnwashedMuddyCar_PaysHalfOfTheNormalTotal()
        {
            var b = Explain(10, open: Muddy);
            Assert.AreEqual(Half(Explain(10).Total), b.Total);
            Assert.IsTrue(b.ServiceMissed);
        }

        [Test] public void UnchargedEv_PaysHalf() => Assert.AreEqual(Half(Explain(10).Total), Explain(10, open: Charge).Total);
        [Test] public void FullyServicedCar_IsNotFlagged() => Assert.IsFalse(Explain(10, cleared: Muddy.With(ModifierId.NeedsCharge)).ServiceMissed);

        [Test]
        public void MuddyAndUncharged_HalveOnce() =>
            Assert.AreEqual(Half(Explain(10).Total), Explain(10, open: Muddy.With(ModifierId.NeedsCharge)).Total);

        [Test]
        public void FakeExtraModifier_HalvesWithTheSameRule()
        {
            Assert.AreEqual(Half(Explain(10).Total), Explain(10, open: ModifierSet.Of(FakeService)).Total);
            Assert.AreEqual(Half(Explain(10).Total), Explain(10, open: Muddy.With(FakeService)).Total);
        }

        // Base minus 2 plus the speed bonus of 2 makes the normal total equal the case value.
        [TestCase(1, 1)]
        [TestCase(2, 1)]
        [TestCase(3, 2)]
        [TestCase(4, 2)]
        [TestCase(5, 3)]
        [TestCase(25, 13)]
        public void Rounding_IsNearestWithTiesUp(int total, int paid)
        {
            Assert.AreEqual(total, Explain(total - 2).Total);
            Assert.AreEqual(paid, Explain(total - 2, open: Muddy).Total);
        }

        [Test]
        public void HalvingComesAfterDamageAndFavour()
        {
            var full = Explain(40, damage: 0.25f, favour: 3).Total;
            Assert.AreEqual(Half(full), Explain(40, open: Muddy, damage: 0.25f, favour: 3).Total);
        }

        [Test]
        public void ClearedModifiersStillPayTheirBonus_AndTheMissedOneNone()
        {
            var b = Explain(10, open: Muddy, cleared: Charge);
            Assert.Greater(b.ModifierBonus, 0);
            Assert.AreEqual(Explain(10, cleared: Charge).ModifierBonus, b.ModifierBonus);
            Assert.AreEqual(Half(Explain(10, cleared: Charge).Total), b.Total);
        }

        [Test]
        public void ZeroReasons_AreNotFlaggedAsMissed()
        {
            Assert.IsFalse(_calc.Explain(new BlessingInput(10, 5f, false, pickupExpired: true, openModifiers: Muddy)).ServiceMissed);
            Assert.IsFalse(Explain(10, open: Muddy, damage: 1f).ServiceMissed);
        }

        [Test]
        public void MissedServiceHandback_AddsBlessingsButNeitherRaisesNorResetsTheFavour()
        {
            var bus = new EventBus();
            var config = ScriptableObject.CreateInstance<ShiftConfig>();
            using (var shift = new ShiftBlessings(bus, config))
            {
                bus.Publish(new ShiftStarted());
                bus.Publish(new BlessingEarned(new TicketId(1), 10));
                var before = shift.FavourMultiplier;
                Assert.Greater(before, 1);
                bus.Publish(new BlessingEarned(new TicketId(2), 6, serviceMissed: true));
                Assert.AreEqual(16, shift.Blessings);
                Assert.AreEqual(before, shift.FavourMultiplier);
                bus.Publish(new BlessingEarned(new TicketId(3), 0, serviceMissed: true));
                Assert.AreEqual(1, shift.FavourMultiplier, "a zero blessing still resets");
            }
            Object.DestroyImmediate(config);
        }
    }
}
