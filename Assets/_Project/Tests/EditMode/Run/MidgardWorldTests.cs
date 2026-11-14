using NUnit.Framework;
using UncannyValet.Core.Art;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Watchers;
using UncannyValet.Lot;
using UncannyValet.Run;
using UncannyValet.UI.Kit;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.Tests.Run
{
    /// <summary>The two shipped Midgard lots (task 106): no services, eight bays, crowd and traffic, props, gag lines and what they seed.</summary>
    public sealed class MidgardWorldTests
    {
        private const string TrainingPath = "Assets/_Project/Data/Run/MidgardTrainingLot.asset";
        private const string CrazyPath = "Assets/_Project/Data/Run/MidgardCrazyLot.asset";
        private const string LotConfigPath = "Assets/_Project/Data/Lot/LotConfig.asset";

        private static LotDefinition Lot(string path) => AssetDatabase.LoadAssetAtPath<LotDefinition>(path);

        private static LotGeometry Geometry(LotDefinition lot)
            => new LotGeometry(AssetDatabase.LoadAssetAtPath<LotConfig>(LotConfigPath), lot.QueueSlots, lot.LayoutOptions);

        private static TuningStore Seeded(LotDefinition lot)
        {
            var tuning = new TuningStore(new EventBus());
            tuning.Seed(TuningKey.TrafficMinOnScreen, 2f);
            tuning.Seed(TuningKey.TrafficTargetOnScreen, 4f);
            tuning.Seed(TuningKey.PlanLateEvChance, 0.5f);
            lot.SeedPacing(tuning, 1f);
            return tuning;
        }

        [TestCase(TrainingPath)]
        [TestCase(CrazyPath)]
        public void BothLots_AreValid_WithServicesClosed_AndEightBays(string path)
        {
            var lot = Lot(path);
            Assert.IsTrue(lot.TryValidate(out var problem), problem);
            Assert.IsTrue(lot.LayoutOptions.ServicesClosed);
            var g = Geometry(lot);
            Assert.AreEqual(8, g.Spots.Length);
            Assert.AreEqual(0, g.ChargingSpots.Length);
            Assert.IsFalse(g.HasWash);
        }

        [TestCase(TrainingPath)]
        [TestCase(CrazyPath)]
        public void NoCarArrivesWithAService_NotEvenInTheLateSurge(string path)
        {
            var lot = Lot(path);
            foreach (var entry in lot.ModifierChances) Assert.AreEqual(0f, entry.Chance, entry.Modifier.name);
            Assert.AreEqual(0f, Seeded(lot).Get(TuningKey.PlanLateEvChance), "the end-of-shift charge surge is off too");
        }

        [Test]
        public void TheTrainingLot_HasNoAmbientTraffic_ThreeWalkers_NoCurses_NoGrant()
        {
            var lot = Lot(TrainingPath);
            var tuning = Seeded(lot);
            Assert.IsFalse(lot.AmbientTraffic);
            Assert.AreEqual(0f, tuning.Get(TuningKey.TrafficTargetOnScreen));
            Assert.AreEqual(0f, tuning.Get(TuningKey.TrafficMinOnScreen));
            Assert.AreEqual(3, lot.AmbientPedestrians);
            Assert.AreEqual(3f, tuning.Get(TuningKey.PedestrianCount));
            Assert.IsFalse(lot.PedestrianCurses);
            Assert.IsTrue(lot.Grant.IsNone);
            Assert.AreEqual(3, lot.QueueSlots);
        }

        [Test]
        public void TheCrazyLot_KeepsTodaysTraffic_TenWalkers_Curses_AndGrantsThunderStride()
        {
            var lot = Lot(CrazyPath);
            var tuning = Seeded(lot);
            Assert.IsTrue(lot.AmbientTraffic);
            Assert.AreEqual(4f, tuning.Get(TuningKey.TrafficTargetOnScreen), "the shared traffic numbers are left alone");
            Assert.AreEqual(10, lot.AmbientPedestrians);
            Assert.AreEqual(10f, tuning.Get(TuningKey.PedestrianCount));
            Assert.IsTrue(lot.PedestrianCurses);
            Assert.AreEqual(StoryPower.ThunderStride, lot.Grant.Power);
            Assert.AreEqual(1, lot.Grant.Level);
            Assert.AreEqual(4, lot.QueueSlots);
        }

        [Test]
        public void TheCrowdPool_HoldsTheBiggestCrowd()
        {
            var config = AssetDatabase.LoadAssetAtPath<UncannyValet.Pedestrians.PedestrianConfig>("Assets/_Project/Data/Pedestrians/PedestrianConfig.asset");
            Assert.GreaterOrEqual(config.Count, Lot(CrazyPath).AmbientPedestrians);
        }

        [Test]
        public void TheTrainingLot_HasAboutFourTrees_AndTheCrazyLotHasStallsAndTheWheel()
        {
            Assert.AreEqual(4, Count(Lot(TrainingPath), "MidgardTree"));
            Assert.GreaterOrEqual(Count(Lot(CrazyPath), "MidgardStall"), 4);
            Assert.AreEqual(1, Count(Lot(CrazyPath), "MidgardFerris"));
            Assert.AreEqual(0, Count(Lot(TrainingPath), "MidgardFerris"));
        }

        [Test]
        public void OnlyTheWheelIsSolid_AmongTheCrazyLotsProps()
        {
            foreach (var prop in Lot(CrazyPath).Props)
                Assert.AreEqual(prop.Recipe.name == "MidgardFerrisWheel", prop.Recipe.Solid, prop.Recipe.name);
        }

        [Test]
        public void TheWheel_IsInsideTheLot_AndTakesNoBayAndBlocksNoAisle()
        {
            var lot = Lot(CrazyPath);
            var g = Geometry(lot);
            var wheel = Find(lot, "MidgardFerrisWheel");
            var block = Rect.MinMaxRect(wheel.Position.x - wheel.Recipe.ColliderSize.x / 2, wheel.Position.y - wheel.Recipe.ColliderSize.y / 2,
                wheel.Position.x + wheel.Recipe.ColliderSize.x / 2, wheel.Position.y + wheel.Recipe.ColliderSize.y / 2);
            Assert.IsTrue(g.Bounds.Contains(block.min) && g.Bounds.Contains(block.max), "the wheel is inside the lot frame");
            foreach (var spot in g.Spots) Assert.IsFalse(block.Overlaps(spot), "the wheel sits on a bay");
            foreach (var car in g.QueueCars) Assert.IsFalse(block.Overlaps(car), "the wheel sits on the queue");
            Assert.IsFalse(block.Overlaps(g.Curb));
            Assert.Less(block.yMax, g.Spots[7].yMin - 1f, "a car can still pass between the bottom bays and the wheel");
            Assert.Less(block.yMax, g.AisleY - 3f, "the aisle is clear");
        }

        [Test]
        public void ThePropsOfBothLots_AreOutsideTheBays_TheAisleAndTheQueue()
        {
            foreach (var path in new[] { TrainingPath, CrazyPath })
            {
                var lot = Lot(path);
                var g = Geometry(lot);
                foreach (var prop in lot.Props)
                {
                    if (prop.Recipe.name == "MidgardOpenSign") continue;
                    var size = new Vector2(prop.Recipe.SizeUnits.x, prop.Recipe.SizeUnits.y);
                    var box = LotGeometry.Box(prop.Position, size);
                    foreach (var spot in g.Spots) Assert.IsFalse(box.Overlaps(spot), prop.Recipe.name + " covers a bay in " + lot.name);
                    foreach (var car in g.QueueCars) Assert.IsFalse(box.Overlaps(car), prop.Recipe.name + " covers the queue in " + lot.name);
                    Assert.IsFalse(box.Overlaps(g.Curb), prop.Recipe.name + " covers the curb");
                    Assert.IsFalse(box.Overlaps(Rect.MinMaxRect(box.xMin, g.AisleY - 0.7f, box.xMax, g.AisleY + 0.7f)), prop.Recipe.name + " is on the aisle crossing");
                }
            }
        }

        [Test]
        public void TheWheelLight_IsWarm_AndTheNightPresetStaysInTheCap()
        {
            var night = AssetDatabase.LoadAssetAtPath<LightingPreset>("Assets/_Project/Data/Art/Midgard/MidgardNight.asset");
            Assert.IsTrue(night.TryValidate(out var problem), problem);
            var wheel = Find(Lot(CrazyPath), "MidgardFerrisWheel");
            var found = false;
            foreach (var light in night.Lights)
                if (Vector2.Distance(light.Position, wheel.Position) < 2f && light.Colour.r > light.Colour.b) found = true;
            Assert.IsTrue(found, "a warm point light at the wheel");
        }

        [TestCase(TrainingPath, "PATRON", "THOR")]
        [TestCase(CrazyPath, "FAIRGOER", "COSTUME")]
        public void EachLot_HasAboutEightGagLines_WithTheRightVoice(string path, string speaker, string theme)
        {
            var gag = Lot(path).GagLines;
            Assert.IsNotNull(gag);
            Assert.IsNull(gag.Check(), gag.Check());
            Assert.AreEqual(speaker, gag.Speaker);
            Assert.AreEqual(8, gag.Lines.Count);
            Assert.AreEqual(3f, gag.TriggerDistance);
            Assert.AreEqual(6f, gag.GlobalLockoutSeconds);
            Assert.AreEqual(20f, gag.WalkerLockoutSeconds);
            var themed = false;
            foreach (var line in gag.Lines) themed |= line.Contains("THOR") || line.Contains(theme);
            Assert.IsTrue(themed);
        }

        [Test]
        public void TheAllowedGagCharacters_AreExactlyThePixelFontsGlyphs() => Assert.AreEqual(PixelGlyphs.Chars, GagLineAsset.AllowedCharacters);

        [Test]
        public void AGagAsset_IsRejected_WithoutLines_OrWithALongLine_OrAnUnknownCharacter()
        {
            Assert.IsNotNull(GagLineAsset.Create("None", "X", new string[0]).Check());
            Assert.IsNotNull(GagLineAsset.Create("Long", "X", new[] { new string('A', GagLineAsset.MaxLineLength + 1) }).Check());
            Assert.IsNotNull(GagLineAsset.Create("Lower", "X", new[] { "hello" }).Check());
            Assert.IsNotNull(GagLineAsset.Create("NoName", "", new[] { "HI" }).Check());
            Assert.IsNull(GagLineAsset.Create("Fine", "X", new[] { "HI, THOR! (1/2) 5%" }).Check());
        }

        [Test]
        public void ALotWithABadGagAsset_FailsItsValidation()
        {
            using (var fx = new RunFixtures())
            {
                var lot = fx.Lot(UncannyValet.Core.Ids.World.Midgard, UncannyValet.Core.Ids.LotKind.Crazy);
                var bad = GagLineAsset.Create("Bad", "X", new string[0]);
                RunFixtures.SetObject(lot, "_gagLines", bad);
                Assert.IsFalse(lot.TryValidate(out var problem));
                StringAssert.Contains("no lines", problem);
                Object.DestroyImmediate(bad);
            }
        }

        [Test]
        public void ANewLot_KeepsTodaysDiner_AndTheSharedTrafficNumbers()
        {
            using (var fx = new RunFixtures())
            {
                var lot = fx.Lot(UncannyValet.Core.Ids.World.Midgard, UncannyValet.Core.Ids.LotKind.Training);
                Assert.IsFalse(lot.LayoutOptions.ServicesClosed);
                Assert.IsTrue(lot.AmbientTraffic);
                var tuning = new TuningStore(new EventBus());
                tuning.Seed(TuningKey.TrafficTargetOnScreen, 4f);
                lot.SeedPacing(tuning, 1f);
                Assert.AreEqual(4f, tuning.Get(TuningKey.TrafficTargetOnScreen));
            }
        }

        private static int Count(LotDefinition lot, string recipePrefix)
        {
            var count = 0;
            foreach (var prop in lot.Props) if (prop.Recipe.name.StartsWith(recipePrefix)) count++;
            return count;
        }

        private static PropPlacement Find(LotDefinition lot, string recipeName)
        {
            foreach (var prop in lot.Props) if (prop.Recipe.name == recipeName) return prop;
            Assert.Fail("no prop " + recipeName);
            return default;
        }
    }
}
