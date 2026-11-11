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
        public void ShippedDefaults_AgreeWithEachOther()
        {
            var lot = ScriptableObject.CreateInstance<LotConfig>();
            var handling = ScriptableObject.CreateInstance<CarHandlingConfig>();
            try { Assert.IsFalse(HandlingAisleCheck.Differs(lot.AisleWidth, handling.MeasuredAisleWidth)); }
            finally { Object.DestroyImmediate(lot); Object.DestroyImmediate(handling); }
        }
    }
}
