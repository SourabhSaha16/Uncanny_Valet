using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.Run;
using UnityEditor;

namespace UncannyValet.Tests.Run
{
    /// <summary>
    /// The shipped lot assets: the HUD power bar is hidden in both Midgard lots and shown in Vanaheim (#137), and every
    /// Vanaheim lot adds power for a Perfect Park (#136). Perfect Park judging itself is not gated by world.
    /// </summary>
    public class PowerBarFlagAssetTests
    {
        private const string Folder = "Assets/_Project/Data/Run/";

        [TestCase("MidgardCrazyLot.asset", false)]
        [TestCase("MidgardTrainingLot.asset", false)]
        [TestCase("VanaheimCrazyLot.asset", true)]
        [TestCase("VanaheimTrainingLot.asset", true)]
        public void TheLot_ShowsThePowerBarOutsideMidgard(string file, bool expected)
        {
            var lot = AssetDatabase.LoadAssetAtPath<LotDefinition>(Folder + file);
            Assert.IsNotNull(lot, file);
            Assert.AreEqual(expected, lot.ShowPowerBar);
        }

        [TestCase("VanaheimCrazyLot.asset")]
        [TestCase("VanaheimTrainingLot.asset")]
        public void TheVanaheimLot_SeedsPowerForAPerfectPark(string file)
        {
            var lot = AssetDatabase.LoadAssetAtPath<LotDefinition>(Folder + file);
            Assert.Greater(lot.PowerFill.PerfectPark, 0f, file);
            Assert.Greater(lot.PowerFill.PerfectDropOff, 0f, file);
        }

        [Test]
        public void ALotGoal_ShowsTheBarByDefault_AndCarriesTheLotFlag()
        {
            var id = new LotId(World.Midgard, LotKind.Crazy);
            Assert.IsTrue(new LotGoal(id, 10).ShowPowerBar);
            Assert.IsFalse(((ILotGoal)new LotGoal(id, 10, false)).ShowPowerBar);
        }
    }
}
