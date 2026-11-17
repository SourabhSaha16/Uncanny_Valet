using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Powers;
using UncannyValet.Valet;
using UnityEditor;

namespace UncannyValet.Tests.Valet
{
    /// <summary>Issue 176: Thunder stride must stay clearly faster than the Shift dash on foot and than normal driving in a car, from the shipped assets.</summary>
    public class ThunderSpeedOrderTests
    {
        private const string PowerPath = "Assets/_Project/Data/Powers/ThunderStrideDefinition.asset";
        private const string CarPath = "Assets/_Project/Data/Cars/Sedan.asset";
        private const string HandlingPath = "Assets/_Project/Data/Cars/CarHandlingConfig.asset";
        private const float MinDashShare = 1.2f;
        private const float MinCarShare = 1.3f;

        private static float DashSpeed => ShiftDash.DefaultDistance / ShiftDash.DefaultSeconds;

        [TestCase(1)]
        [TestCase(2)]
        public void OnFootWalkIsSlowerThanDashAndDashSlowerThanThunder(int level)
        {
            var definition = AssetDatabase.LoadAssetAtPath<PowerDefinition>(PowerPath);
            Assert.IsTrue(definition.TryGetValues(level, out var values));
            var thunder = OnFootMover.DefaultSpeed * values.Secondary;
            Assert.Less(OnFootMover.DefaultSpeed, DashSpeed);
            Assert.GreaterOrEqual(thunder, DashSpeed * MinDashShare, "Thunder stride on foot vs the Shift dash speed");
        }

        [TestCase(1)]
        [TestCase(2)]
        public void TheCarsBoostedTopSpeedIsClearlyAboveItsNormalTopSpeed(int level)
        {
            var definition = AssetDatabase.LoadAssetAtPath<PowerDefinition>(PowerPath);
            var car = AssetDatabase.LoadAssetAtPath<CarDefinition>(CarPath);
            var live = new CarHandlingLive(AssetDatabase.LoadAssetAtPath<CarHandlingConfig>(HandlingPath));
            Assert.IsTrue(definition.TryGetValues(level, out var values));
            var normal = live.DriveFor(car).TopSpeed;
            var boosted = new ThunderBoostModifier(values.PushStrength, values.Width).Modify(new DriveInput(1f, 0f, false));
            var cap = normal * boosted.TopSpeedScale;
            Assert.GreaterOrEqual(cap, normal * MinCarShare);
            var reached = CarPhysicsMath.NextForwardSpeed(cap, 1f, 100f, 12f, 3f, cap, 0.4f, 0.02f);
            Assert.Greater(reached, normal * MinCarShare, "the real clamp lets the boosted speed through");
        }
    }
}
