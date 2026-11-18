using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.UI
{
    /// <summary>Issue 189: Drop off is where the patron leaves the car, Pickup is where the valet brings it back.</summary>
    public class PlayerWordingTests
    {
        [Test]
        public void CurbText_SaysPickup_AndTheFirstQueueSlotSaysDropOff()
        {
            var config = ScriptableObject.CreateInstance<LotConfig>();
            try
            {
                CollectionAssert.AreEqual(new[] { "VALET", "PICKUP" }, config.CurbLabelWords);
                CollectionAssert.AreEqual(new[] { "VALET", "DROP-OFF" }, config.PickupLabelWords);
            }
            finally { Object.DestroyImmediate(config); }
        }

        [Test]
        public void PerfectRows_CallDeliveringTheCarAtTheCurbAPickup()
        {
            foreach (var row in PerfectRows.Rows)
                StringAssert.DoesNotContain("drop-off", row.Label.ToLowerInvariant());
            StringAssert.Contains("pickup", PerfectRows.Rows[0].Label.ToLowerInvariant());
        }
    }
}
