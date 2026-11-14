using NUnit.Framework;
using UncannyValet.Core.Art;
using UncannyValet.Lot;
using UncannyValet.Run;
using UnityEngine;
using static UncannyValet.Tests.Run.VanaheimAssets;

namespace UncannyValet.Tests.Run
{
    /// <summary>The shipped Vanaheim lots (task 107): palette, props, lighting, the barn sign and the dirt ground.</summary>
    public sealed class VanaheimArtTests
    {
        [Test]
        public void ThePropsAndLighting_ComeFromTheVanaheimPalette_WithTheRightPresets()
        {
            foreach (var path in new[] { TrainingPath, CrazyPath })
            {
                var lot = LotAt(path);
                Assert.AreEqual("VanaheimPalette", lot.Palette.name);
                foreach (var prop in lot.Props) StringAssert.StartsWith("Vanaheim", prop.Recipe.name);
                Assert.IsTrue(lot.Lighting.TryValidate(out var problem), problem);
                Assert.LessOrEqual(lot.Lighting.Lights.Count, LightingPreset.MaxLights);
            }
            Assert.AreEqual("VanaheimSunnyAfternoon", LotAt(TrainingPath).Lighting.name);
            Assert.AreEqual("VanaheimStormyEvening", LotAt(CrazyPath).Lighting.name);
            Assert.Greater(LotAt(TrainingPath).Lighting.GlobalIntensity, LotAt(CrazyPath).Lighting.GlobalIntensity, "evening is dimmer");
        }

        [Test]
        public void TheSunnyLight_StaysAtOrBelowFullIntensity_AboveOneTheRenderBlowsEverythingOutToCream()
            => Assert.LessOrEqual(LotAt(TrainingPath).Lighting.GlobalIntensity, 1f);

        [Test]
        public void TheStormyLights_AreHungOverTheVanaheimLayout_InsideTheLot_AndATroughGlintEach()
        {
            var lot = LotAt(CrazyPath);
            var g = Geometry(lot);
            foreach (var light in lot.Lighting.Lights) Assert.IsTrue(g.Bounds.Contains(light.Position), "light at " + light.Position);
            foreach (var trough in g.Troughs)
            {
                var near = false;
                foreach (var light in lot.Lighting.Lights) near |= Vector2.Distance(light.Position, trough.center) < 1f;
                Assert.IsTrue(near, "a light on the trough at " + trough.center);
            }
        }

        [Test]
        public void TheBarnAndTheTroughs_ArePlaced_TheFarmSceneryIsDecor()
        {
            Assert.AreEqual(1, Count(LotAt(TrainingPath), "VanaheimBarn"));
            Assert.AreEqual(1, Count(LotAt(TrainingPath), "VanaheimTrough"));
            Assert.Greater(Count(LotAt(TrainingPath), "VanaheimWheat"), 20);
            Assert.GreaterOrEqual(Count(LotAt(TrainingPath), "VanaheimHayBale"), 2);
            Assert.GreaterOrEqual(Count(LotAt(TrainingPath), "VanaheimFence"), 4);
            Assert.AreEqual(3, Count(LotAt(CrazyPath), "VanaheimTrough"));
            Assert.GreaterOrEqual(Count(LotAt(CrazyPath), "VanaheimStall"), 4);
            Assert.GreaterOrEqual(Count(LotAt(CrazyPath), "VanaheimBanner"), 3);
            foreach (var path in new[] { TrainingPath, CrazyPath })
                foreach (var prop in LotAt(path).Props)
                    Assert.AreEqual(prop.Recipe.name == "VanaheimBarn", prop.Recipe.Solid, prop.Recipe.name + " solid");
        }

        [Test]
        public void ThePropsOfBothLots_StayOffTheBays_TheAisleCrossing_TheQueueAndTheCurb()
        {
            foreach (var path in new[] { TrainingPath, CrazyPath })
            {
                var lot = LotAt(path);
                var g = Geometry(lot);
                foreach (var prop in lot.Props)
                {
                    var trough = prop.Recipe.name == "VanaheimTrough";
                    var size = trough ? new Vector2(3.4f, 3f) : new Vector2(prop.Recipe.SizeUnits.x, prop.Recipe.SizeUnits.y);
                    var box = LotGeometry.Box(prop.Position, size);
                    if (prop.Recipe.name != "VanaheimBarn") // the barn stands on the building
                    {
                        foreach (var spot in g.Spots) Assert.IsFalse(box.Overlaps(spot), prop.Recipe.name + " covers a bay in " + lot.name);
                        Assert.IsFalse(box.Overlaps(Rect.MinMaxRect(box.xMin, g.AisleY - 0.7f, box.xMax, g.AisleY + 0.7f)), prop.Recipe.name + " is on the aisle crossing");
                    }
                    foreach (var car in g.QueueCars) Assert.IsFalse(box.Overlaps(car), prop.Recipe.name + " covers the queue in " + lot.name);
                    Assert.IsFalse(box.Overlaps(g.Curb), prop.Recipe.name + " covers the curb");
                    Assert.IsTrue(g.Bounds.Contains(box.min) && g.Bounds.Contains(box.max), prop.Recipe.name + " is inside the lot " + lot.name);
                }
            }
        }

        [Test]
        public void TheLots_HaveABarnSignAndADirtGround_AndMidgardKeepsItsDiner()
        {
            Assert.AreEqual("BARN", LotAt(TrainingPath).BuildingSign);
            Assert.AreEqual("BARN", LotAt(CrazyPath).BuildingSign);
            Assert.Greater(LotAt(TrainingPath).GroundColour.a, 0f);
            Assert.Greater(LotAt(CrazyPath).GroundColour.a, 0f);
            var midgard = LotAt("Assets/_Project/Data/Run/MidgardCrazyLot.asset");
            Assert.AreEqual("DINER", midgard.BuildingSign);
            Assert.AreEqual(0f, midgard.GroundColour.a);
        }

        private static int Count(LotDefinition lot, string recipeName)
        {
            var count = 0;
            foreach (var prop in lot.Props) if (prop.Recipe.name == recipeName) count++;
            return count;
        }
    }
}
