using UncannyValet.Core.Contracts;
using UncannyValet.Run;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>Start-up check that the built lot has the queue slots its LotDefinition asks for.</summary>
    public static class QueueSlotCheck
    {
        /// <summary>True when the lot has <see cref="LotDefinition.QueueSlots"/> slots; otherwise logs the mismatch and returns false.</summary>
        public static bool Verify(LotDefinition definition, ILotLayout lot, Object context)
        {
            if (lot.ArrivalSlots.Count == definition.QueueSlots) return true;
            Debug.LogError("Lot has " + lot.ArrivalSlots.Count + " queue slots but " + definition.name + " asks for "
                + definition.QueueSlots + ": rebuild with Uncanny Valet/Setup/Build Everything.", context);
            return false;
        }
    }
}
