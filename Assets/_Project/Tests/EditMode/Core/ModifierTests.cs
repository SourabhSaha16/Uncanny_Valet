using System;
using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UncannyValet.UI.Kit;
using UnityEditor;

namespace UncannyValet.Tests.Core
{
    public class ModifierTests
    {
        [Test]
        public void ModifierSet_HoldsSeveralIds_AndRoundTripsThem()
        {
            var set = ModifierSet.Of(ModifierId.NeedsCharge, ModifierId.Muddy);
            Assert.AreEqual(2, set.Count);
            Assert.IsTrue(set.Contains(ModifierId.Muddy));
            Assert.IsTrue(set.Contains(ModifierId.NeedsCharge));
            Assert.AreEqual(ModifierId.Muddy, set.At(0), "id order");
            Assert.AreEqual(ModifierId.NeedsCharge, set.At(1));
            Assert.AreEqual(ModifierId.None, set.At(2));
            Assert.AreEqual(ModifierSet.Of(ModifierId.NeedsCharge), set.Without(ModifierId.Muddy));
            Assert.AreEqual(ModifierSet.Of(ModifierId.Muddy), set.Without(ModifierSet.Of(ModifierId.NeedsCharge)));
            Assert.IsTrue(ModifierSet.None.IsEmpty);
            Assert.IsFalse(ModifierSet.None.Contains(ModifierId.None));
            Assert.AreEqual(set, ModifierSet.Of(ModifierId.Muddy, ModifierId.NeedsCharge));
        }

        [Test]
        public void ModifierSet_NoneAddsNothing_AndAnOutOfRangeIdThrows()
        {
            Assert.AreEqual(ModifierSet.None, ModifierSet.None.With(ModifierId.None));
            Assert.Throws<ArgumentOutOfRangeException>(() => ModifierSet.None.With((ModifierId)ModifierSet.Capacity));
        }

        [Test]
        public void ModifierIds_AreAppendOnly()
        {
            Assert.AreEqual(0, (int)ModifierId.None);
            Assert.AreEqual(1, (int)ModifierId.Muddy);
            Assert.AreEqual(2, (int)ModifierId.NeedsCharge);
        }

        [Test]
        public void TheCatalog_FindsByIdKeepsOrder_AndSkipsDuplicatesAndNulls()
        {
            var charge = ServiceModifierDefinition.Create(ModifierDefaults.NeedsCharge());
            var muddy = ServiceModifierDefinition.Create(ModifierDefaults.Muddy());
            var catalog = new ModifierCatalog(new[] { charge, null, muddy, muddy });
            Assert.AreEqual(2, catalog.All.Count);
            Assert.AreSame(charge, catalog.All[0]);
            Assert.IsTrue(catalog.TryGet(ModifierId.Muddy, out var found));
            Assert.AreSame(muddy, found);
            Assert.IsFalse(new ModifierCatalog(new[] { charge }).TryGet(ModifierId.Muddy, out _));
            Assert.IsFalse(catalog.TryGet(ModifierId.None, out _));
        }

        [Test]
        public void MuddyAndNeedsCharge_CarryTheOldWashAndEvNumbers()
        {
            var catalog = ModifierTestKit.Catalog();
            catalog.TryGet(ModifierId.Muddy, out var muddy);
            catalog.TryGet(ModifierId.NeedsCharge, out var charge);
            Assert.AreEqual(StationKind.Wash, muddy.ClearedAt);
            Assert.AreEqual(15f, muddy.ClearMinutes);
            Assert.AreEqual(0f, muddy.StartProgress01);
            Assert.AreEqual(15, muddy.Bonus);
            Assert.AreEqual(StationKind.Charging, charge.ClearedAt);
            Assert.AreEqual(20f, charge.ClearMinutes);
            Assert.AreEqual(0.2f, charge.StartProgress01);
            Assert.AreEqual(15, charge.Bonus);
            Assert.IsTrue(charge.RampsLate);
            Assert.IsFalse(muddy.RampsLate);
        }

        [Test]
        public void TheStatusWordsComeFromTheDefinitions()
        {
            var catalog = ModifierTestKit.Catalog();
            catalog.TryGet(ModifierId.Muddy, out var muddy);
            catalog.TryGet(ModifierId.NeedsCharge, out var charge);
            Assert.AreEqual("Washing", muddy.ActiveWord);
            Assert.AreEqual("Washed", muddy.DoneWord);
            Assert.AreEqual("Charging", charge.ActiveWord);
            Assert.AreEqual("Charged", charge.DoneWord);
        }

        [Test]
        public void EveryModifierHasItsOwnIconWithABitmap()
        {
            var catalog = ModifierTestKit.Catalog();
            var seen = new System.Collections.Generic.HashSet<PixelIcon>();
            foreach (var def in catalog.All)
            {
                var icon = ModifierIcons.ToPixel(def.Icon);
                Assert.IsTrue(seen.Add(icon), def.DisplayName + " shares its icon");
                Assert.Greater(PixelIconData.Rows(icon).Length, 0, def.DisplayName);
            }
            Assert.AreEqual(PixelIcon.Mud, ModifierIcons.ToPixel(ModifierIcon.Mud));
            Assert.AreEqual(PixelIcon.Plug, ModifierIcons.ToPixel(ModifierIcon.Plug));
        }

        [Test]
        public void TheIconKeysAreAllInThePixelPalette()
        {
            foreach (var row in PixelIconData.Rows(PixelIcon.Mud))
                foreach (var key in row)
                    Assert.IsTrue(key == '.' || PixelIconData.Keys.IndexOf(key) >= 0, "unknown icon key " + key);
        }

        [TestCase("Assets/_Project/Data/Modifiers/Muddy.asset", ModifierId.Muddy)]
        [TestCase("Assets/_Project/Data/Modifiers/NeedsCharge.asset", ModifierId.NeedsCharge)]
        public void TheShippedDefinitionAssets_MatchTheDefaults(string path, ModifierId id)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ServiceModifierDefinition>(path);
            Assert.IsNotNull(asset, path);
            Assert.AreEqual(id, asset.Id);
            ModifierTestKit.Catalog().TryGet(id, out var expected);
            Assert.AreEqual(expected.ClearedAt, asset.ClearedAt);
            Assert.AreEqual(expected.ClearMinutes, asset.ClearMinutes);
            Assert.AreEqual(expected.StartProgress01, asset.StartProgress01);
            Assert.AreEqual(expected.Bonus, asset.Bonus);
            Assert.AreEqual(expected.ActiveWord, asset.ActiveWord);
            Assert.AreEqual(expected.DoneWord, asset.DoneWord);
            Assert.AreEqual(expected.ChanceKey, asset.ChanceKey);
        }

        [Test]
        public void TheLooksAreFoundByCatalog()
        {
            var catalog = ModifierTestKit.Catalog();
            Assert.IsTrue(ModifierLooks.Has(catalog, ModifierSet.Of(ModifierId.NeedsCharge), ModifierLook.ElectricBody));
            Assert.IsFalse(ModifierLooks.Has(catalog, ModifierSet.Of(ModifierId.Muddy), ModifierLook.ElectricBody));
            Assert.IsTrue(ModifierLooks.Has(catalog, ModifierSet.Of(ModifierId.Muddy), ModifierLook.MudOverlay));
            Assert.IsTrue(ModifierLooks.Is(catalog, ModifierId.Muddy, ModifierLook.MudOverlay));
            Assert.IsFalse(ModifierLooks.Has(null, ModifierSet.Of(ModifierId.Muddy), ModifierLook.MudOverlay));
        }
    }
}
