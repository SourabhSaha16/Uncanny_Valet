using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// How many people the reserved patron pool needs: one per car that can hold an open ticket at once, which is every
    /// parking spot plus every arrival queue slot. A configured floor keeps the pool from shrinking below a safe minimum.
    /// </summary>
    public static class PatronPoolSizing
    {
        /// <summary>The pool size for a lot with <paramref name="spotCount"/> spots and <paramref name="queueSlotCount"/> queue slots.</summary>
        public static int Compute(int floor, int spotCount, int queueSlotCount)
            => Mathf.Max(Mathf.Max(0, floor), Mathf.Max(0, spotCount) + Mathf.Max(0, queueSlotCount));
    }
}
