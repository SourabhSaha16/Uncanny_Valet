using UnityEditor;
using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Cars;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    public class HandlingAisleCheckTests
    {
        [Test]
        public void Differs_IsFalseForEqualWidthsAndTrueForAMismatch()
        {
            Assert.IsFalse(HandlingAisleCheck.Differs(4.6f, 4.6f));
            Assert.IsTrue(HandlingAisleCheck.Differs(4.6f, 6f));
        }

        [Test]
        public void ShippedAssets_AgreeWithEachOther()
        {
            var lot = AssetDatabase.LoadAssetAtPath<LotConfig>("Assets/_Project/Data/Lot/LotConfig.asset");
            var handling = AssetDatabase.LoadAssetAtPath<CarHandlingConfig>("Assets/_Project/Data/Cars/CarHandlingConfig.asset");
            Assert.IsNotNull(lot);
            Assert.IsNotNull(handling);
            Assert.IsFalse(HandlingAisleCheck.Differs(lot.AisleWidth, handling.MeasuredAisleWidth));
        }
    }
}
