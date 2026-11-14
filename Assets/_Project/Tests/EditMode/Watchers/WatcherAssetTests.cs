using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Watchers;
using UncannyValet.Lot;
using UncannyValet.Pedestrians;
using UncannyValet.Run;
using UncannyValet.UI.Kit;
using UncannyValet.Watchers;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.Tests.Watchers
{
    /// <summary>The shipped watcher assets: every definition valid against the real pedestrian sprite set, every line printable, the Midgard lots using theirs, spots clear of the drivable area.</summary>
    public sealed class WatcherAssetTests
    {
        private const string Folder = "Assets/_Project/Data/Watchers";
        private const string CrazyPath = "Assets/_Project/Data/Run/MidgardCrazyLot.asset";
        private const string TrainingPath = "Assets/_Project/Data/Run/MidgardTrainingLot.asset";
        private const string PedestrianPath = "Assets/_Project/Data/Pedestrians/PedestrianConfig.asset";
        private const string LotConfigPath = "Assets/_Project/Data/Lot/LotConfig.asset";
        private const float Margin = 0.5f;

        private static List<WatcherDefinition> All()
        {
            var list = new List<WatcherDefinition>();
            foreach (var guid in AssetDatabase.FindAssets("t:WatcherDefinition", new[] { Folder }))
                list.Add(AssetDatabase.LoadAssetAtPath<WatcherDefinition>(AssetDatabase.GUIDToAssetPath(guid)));
            return list;
        }

        private static WatcherDefinition Named(string fileName) => AssetDatabase.LoadAssetAtPath<WatcherDefinition>(Folder + "/" + fileName + ".asset");

        [Test]
        public void TheFourFirstCharactersExist()
        {
            Assert.AreEqual(4, All().Count);
            foreach (var name in new[] { "MrsOkafor", "Grandma", "Freyja", "VanirFarmhand" }) Assert.IsNotNull(Named(name), name);
        }

        [Test]
        public void EveryDefinition_IsValid_AgainstTheRealSpriteSet()
        {
            var config = AssetDatabase.LoadAssetAtPath<PedestrianConfig>(PedestrianPath);
            Assert.IsNotNull(config);
            foreach (var watcher in All())
            {
                Assert.IsTrue(watcher.TryValidate(config.Skins, config.Hairs, config.Outfits, out var problem), problem);
            }
        }

        [Test]
        public void EveryLine_UsesOnlyThePixelFontsCharacters()
        {
            foreach (var watcher in All())
                foreach (var line in AllLines(watcher.Text))
                    foreach (var c in line)
                        Assert.GreaterOrEqual(PixelGlyphs.Chars.IndexOf(c), 0, watcher.name + ": '" + c + "' in \"" + line + "\" has no glyph");
        }

        [Test]
        public void EveryLine_IsUppercase()
        {
            foreach (var watcher in All())
                foreach (var line in AllLines(watcher.Text)) Assert.AreEqual(line.ToUpperInvariant(), line, watcher.name);
        }

        [Test]
        public void EachCharacter_HasAboutSixTips_AndThreeLinesPerEvent()
        {
            foreach (var watcher in All())
            {
                Assert.AreEqual(6, watcher.Text.Tips.Count, watcher.name);
                foreach (WatcherEvent e in System.Enum.GetValues(typeof(WatcherEvent)))
                    Assert.AreEqual(3, watcher.Text.Lines(e).Count, watcher.name + " " + e);
            }
        }

        [Test]
        public void Tips_NeverMentionPowers()
        {
            foreach (var watcher in All())
                foreach (var tip in watcher.Text.Tips)
                    foreach (var banned in new[] { "THUNDER", "POWER", "MJOLNIR", "STORM", "FREEZE", "DASH" })
                        StringAssert.DoesNotContain(banned, tip.Text, watcher.name);
        }

        [Test]
        public void MrsOkaforShowsHerName() => Assert.AreEqual("MRS. OKAFOR", Named("MrsOkafor").SpeakerName);

        [Test]
        public void TheMidgardLots_NameTheirWatchers()
        {
            var crazy = AssetDatabase.LoadAssetAtPath<LotDefinition>(CrazyPath);
            var training = AssetDatabase.LoadAssetAtPath<LotDefinition>(TrainingPath);
            Assert.AreEqual(Named("MrsOkafor"), crazy.Watcher);
            Assert.AreEqual(Named("Grandma"), training.Watcher);
            Assert.IsTrue(crazy.TryValidate(out var problem), problem);
            Assert.IsTrue(training.TryValidate(out problem), problem);
        }

        [TestCase(CrazyPath)]
        [TestCase(TrainingPath)]
        public void TheWatcherSpot_IsOutsideTheDrivableArea(string lotPath)
        {
            var lot = AssetDatabase.LoadAssetAtPath<LotDefinition>(lotPath);
            var config = AssetDatabase.LoadAssetAtPath<LotConfig>(LotConfigPath);
            var g = new LotGeometry(config, lot.QueueSlots);
            var spot = lot.WatcherSpot;
            Assert.IsTrue(g.Bounds.Contains(spot), "inside the lot frame");
            foreach (var r in DrivableAreas(g)) Assert.IsFalse(Inflate(r, Margin).Contains(spot), "spot " + spot + " is inside " + r);
        }

        // Where cars drive or park: the curb, the queue, every bay, the aisle strip and the exit lane above the curb.
        private static IEnumerable<Rect> DrivableAreas(LotGeometry g)
        {
            yield return g.Curb;
            yield return g.Corridor;
            yield return g.Wash;
            foreach (var r in g.Spots) yield return r;
            foreach (var r in g.ChargingSpots) yield return r;
            foreach (var r in g.QueueCars) yield return r;
            yield return Rect.MinMaxRect(g.ExitGap.x, g.Curb.yMax, g.ExitGap.y, g.Bounds.yMax);
            yield return Rect.MinMaxRect(g.Spots[0].xMin, g.Bounds.yMin, g.Bounds.xMax, g.Bounds.yMax);
        }

        private static Rect Inflate(Rect r, float by) => Rect.MinMaxRect(r.xMin - by, r.yMin - by, r.xMax + by, r.yMax + by);

        private static IEnumerable<string> AllLines(WatcherTextAsset text)
        {
            foreach (var tip in text.Tips) yield return tip.Text;
            foreach (WatcherEvent e in System.Enum.GetValues(typeof(WatcherEvent)))
                foreach (var line in text.Lines(e)) yield return line;
            foreach (WatcherVerdict v in System.Enum.GetValues(typeof(WatcherVerdict)))
                foreach (var line in text.Verdict(v)) yield return line;
        }
    }
}
