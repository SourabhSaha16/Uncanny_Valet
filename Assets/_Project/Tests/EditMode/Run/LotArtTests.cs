using NUnit.Framework;
using UncannyValet.Core.Art;
using UncannyValet.Core.Ids;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.Tests.Run
{
    /// <summary>A lot definition's palette, props and lighting preset: validation, and the two Midgard lots using them.</summary>
    public sealed class LotArtTests
    {
        private const string TrainingPath = "Assets/_Project/Data/Run/MidgardTrainingLot.asset";
        private const string CrazyPath = "Assets/_Project/Data/Run/MidgardCrazyLot.asset";
        private const string ArtFolder = "Assets/_Project/Data/Art/";

        private RunFixtures _fx;
        private LotDefinition _lot;

        [SetUp]
        public void SetUp()
        {
            _fx = new RunFixtures();
            _lot = _fx.Lot(World.Midgard, LotKind.Training, "Bootstrap", "Diner");
        }

        [TearDown]
        public void TearDown() => _fx.Dispose();

        private static T Load<T>(string name) where T : Object => AssetDatabase.LoadAssetAtPath<T>(ArtFolder + "Midgard/" + name + ".asset");

        private void AddProp(PropRecipe recipe)
        {
            var so = new SerializedObject(_lot);
            var list = so.FindProperty("_props");
            list.arraySize = 1;
            list.GetArrayElementAtIndex(0).FindPropertyRelative("_recipe").objectReferenceValue = recipe;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void ALotWithoutArt_StaysValid()
        {
            Assert.IsTrue(_lot.TryValidate(out var problem), problem);
        }

        [Test]
        public void PropsNeedAPalette_AndAValidRecipe()
        {
            AddProp(Load<PropRecipe>("MidgardTree"));
            Assert.IsFalse(_lot.TryValidate(out var problem));
            StringAssert.Contains("palette", problem);
            var so = new SerializedObject(_lot);
            so.FindProperty("_palette").objectReferenceValue = Load<WorldPalette>("MidgardPalette");
            so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsTrue(_lot.TryValidate(out problem), problem);
            AddProp(null);
            Assert.IsFalse(_lot.TryValidate(out _));
        }

        [Test]
        public void TheMidgardLots_UseThePaletteAndTheirLighting_WithTheOpenSign()
        {
            var training = AssetDatabase.LoadAssetAtPath<LotDefinition>(TrainingPath);
            var crazy = AssetDatabase.LoadAssetAtPath<LotDefinition>(CrazyPath);
            Assert.AreEqual("MidgardPalette", training.Palette.name);
            Assert.AreEqual("MidgardDay", training.Lighting.name);
            Assert.AreEqual("MidgardNight", crazy.Lighting.name);
            foreach (var lot in new[] { training, crazy })
            {
                Assert.IsTrue(lot.TryValidate(out var problem), problem);
                Assert.AreEqual(1, lot.Props.Count);
                Assert.AreEqual("MidgardOpenSign", lot.Props[0].Recipe.name);
            }
        }
    }
}
