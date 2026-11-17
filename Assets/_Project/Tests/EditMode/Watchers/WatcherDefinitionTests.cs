using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Watchers;
using UncannyValet.Run;
using UncannyValet.Tests.Run;
using UnityEngine;

namespace UncannyValet.Tests.Watchers
{
    /// <summary>Definition validation, the sprite recipe's completeness and the results-screen accessors.</summary>
    public sealed class WatcherDefinitionTests
    {
        private const int Skins = 6;
        private const int Hairs = 8;
        private const int Outfits = 6;

        private static WatcherLook Look(int skin = 0, int hair = 0, int outfit = 0, List<WatcherAccessory> accessories = null)
            => new WatcherLook(skin, hair, outfit, Color.white, Color.white, Color.white,
                accessories ?? new List<WatcherAccessory> { new WatcherAccessory("Box", new Vector2(16f, 16f), new Vector2(2f, 2f), Color.red) });

        private static WatcherTextAsset FullText(bool verdicts)
        {
            var tips = new List<WatcherTip> { new WatcherTip(TipSituation.MuddyCar, "WASH IT") };
            var comments = new Dictionary<WatcherEvent, string[]>();
            foreach (WatcherEvent e in System.Enum.GetValues(typeof(WatcherEvent))) comments[e] = new[] { "LINE" };
            var verdictLines = verdicts ? new Dictionary<WatcherVerdict, string[]>
            {
                [WatcherVerdict.Failed] = new[] { "NO" }, [WatcherVerdict.Cleared] = new[] { "OK", "FINE" }, [WatcherVerdict.ClearedWell] = new[] { "WOW" },
            } : null;
            return WatcherTextAsset.Create("Text", tips, comments, verdictLines);
        }

        private static WatcherDefinition God(WatcherLook look = null, WatcherTextAsset text = null, string speaker = "BOSS")
            => WatcherDefinition.Create("God", World.Midgard, WatcherKind.God, speaker, look ?? Look(), text ?? FullText(true));

        [Test]
        public void CompleteGod_IsValid() => Assert.IsTrue(God().TryValidate(Skins, Hairs, Outfits, out var problem), problem);

        [Test]
        public void ATrainer_NeedsNoVerdictLines()
        {
            var trainer = WatcherDefinition.Create("T", World.Midgard, WatcherKind.Trainer, "COACH", Look(), FullText(false));
            Assert.IsTrue(trainer.TryValidate(Skins, Hairs, Outfits, out var problem), problem);
        }

        [Test]
        public void AGod_NeedsVerdictLines() => Assert.IsFalse(God(text: FullText(false)).TryValidate(Skins, Hairs, Outfits, out _));

        [Test]
        public void NeedsASpeakerName() => Assert.IsFalse(God(speaker: "").TryValidate(Skins, Hairs, Outfits, out _));

        [Test]
        public void NeedsAKind()
        {
            var none = WatcherDefinition.Create("N", World.Midgard, WatcherKind.None, "X", Look(), FullText(true));
            Assert.IsFalse(none.TryValidate(Skins, Hairs, Outfits, out _));
        }

        [Test]
        public void EmptyEventList_IsInvalid()
        {
            var text = WatcherTextAsset.Create("Text", new List<WatcherTip> { new WatcherTip(TipSituation.MuddyCar, "X") }, null, null);
            Assert.IsFalse(God(text: text).TryValidate(Skins, Hairs, Outfits, out var problem));
            StringAssert.Contains("no lines", problem);
        }

        [Test]
        public void NoTips_IsInvalid()
        {
            var text = WatcherTextAsset.Create("Text", null, null, null);
            Assert.IsFalse(WatcherRules.CheckText("X", text, false) == null);
        }

        [Test]
        public void ALongLine_IsInvalid()
            => Assert.IsFalse(WatcherRules.IsGoodLine(new string('A', WatcherTextAsset.MaxLineLength + 1)));

        [TestCase(Skins)]
        [TestCase(Skins + 3)]
        public void SkinRowMustExist(int skin) => Assert.IsFalse(Look(skin: skin).TryValidate(Skins, Hairs, Outfits, out _));

        [TestCase(Hairs)]
        public void HairRowMustExist(int hair) => Assert.IsFalse(Look(hair: hair).TryValidate(Skins, Hairs, Outfits, out _));

        [TestCase(Outfits)]
        public void OutfitMustExist(int outfit) => Assert.IsFalse(Look(outfit: outfit).TryValidate(Skins, Hairs, Outfits, out _));

        [Test]
        public void ALookWithoutAccessories_IsIncomplete()
            => Assert.IsFalse(Look(accessories: new List<WatcherAccessory>()).TryValidate(Skins, Hairs, Outfits, out _));

        [Test]
        public void AnInvisibleOrEmptyAccessory_IsIncomplete()
        {
            var empty = new List<WatcherAccessory> { new WatcherAccessory("Gone", Vector2.zero, Vector2.zero, Color.red) };
            var clear = new List<WatcherAccessory> { new WatcherAccessory("Clear", Vector2.zero, Vector2.one, Color.clear) };
            Assert.IsFalse(Look(accessories: empty).TryValidate(Skins, Hairs, Outfits, out _));
            Assert.IsFalse(Look(accessories: clear).TryValidate(Skins, Hairs, Outfits, out _));
        }

        [Test]
        public void VerdictLine_WrapsAndOnlyGodsHaveOne()
        {
            var god = God();
            Assert.IsTrue(god.TryGetVerdictLine(WatcherVerdict.Cleared, 0, out var first));
            Assert.IsTrue(god.TryGetVerdictLine(WatcherVerdict.Cleared, 3, out var wrapped));
            Assert.AreEqual("OK", first);
            Assert.AreEqual("FINE", wrapped);
            var trainer = WatcherDefinition.Create("T", World.Midgard, WatcherKind.Trainer, "COACH", Look(), FullText(true));
            Assert.IsFalse(trainer.TryGetVerdictLine(WatcherVerdict.Cleared, 0, out _));
        }

        [Test]
        public void LotDefinition_AcceptsAMatchingWatcher_AndRejectsOthers()
        {
            var fx = new RunFixtures();
            try
            {
                var crazy = fx.Lot(World.Midgard, LotKind.Crazy);
                var training = fx.Lot(World.Midgard, LotKind.Training);
                var trainer = WatcherDefinition.Create("T", World.Midgard, WatcherKind.Trainer, "COACH", Look(), FullText(false));
                var vanir = WatcherDefinition.Create("V", World.Vanaheim, WatcherKind.God, "FREYJA", Look(), FullText(true));
                RunFixtures.SetObject(crazy, "_watcher", God());
                Assert.IsTrue(crazy.TryValidate(out var problem), problem);
                RunFixtures.SetObject(crazy, "_watcher", trainer);
                Assert.IsFalse(crazy.TryValidate(out _), "a trainer does not watch a crazy lot");
                RunFixtures.SetObject(training, "_watcher", God());
                Assert.IsFalse(training.TryValidate(out _), "a god does not watch a training lot");
                RunFixtures.SetObject(training, "_watcher", trainer);
                Assert.IsTrue(training.TryValidate(out problem), problem);
                RunFixtures.SetObject(training, "_watcher", vanir);
                Assert.IsFalse(training.TryValidate(out _), "a watcher of another world");
            }
            finally { fx.Dispose(); }
        }
    }
}
