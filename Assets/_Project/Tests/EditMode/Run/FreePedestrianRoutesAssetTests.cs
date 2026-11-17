using NUnit.Framework;
using UncannyValet.Run;
using UnityEditor;

namespace UncannyValet.Tests.Run
{
    /// <summary>The shipped lot assets carry the pedestrian free-route switch: on in both crazy lots, off in both training lots. Issue #124.</summary>
    public class FreePedestrianRoutesAssetTests
    {
        private const string Folder = "Assets/_Project/Data/Run/";

        [TestCase("MidgardCrazyLot.asset", true)]
        [TestCase("VanaheimCrazyLot.asset", true)]
        [TestCase("MidgardTrainingLot.asset", false)]
        [TestCase("VanaheimTrainingLot.asset", false)]
        public void TheLot_SetsFreePedestrianRoutesByKind(string file, bool expected)
        {
            var lot = AssetDatabase.LoadAssetAtPath<LotDefinition>(Folder + file);
            Assert.IsNotNull(lot, file);
            Assert.AreEqual(expected, lot.FreePedestrianRoutes);
        }
    }
}
