using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Pure maths for the arrival queue: slots in a straight line, slot 0 first, each one further back (-Y).</summary>
    public static class ArrivalQueue
    {
        public static Vector2[] Slots(Vector2 first, int count, float spacing)
        {
            var slots = new Vector2[Mathf.Max(1, count)];
            for (var i = 0; i < slots.Length; i++) slots[i] = first + new Vector2(0f, -spacing * i);
            return slots;
        }
    }
}
