using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Powers
{
    /// <summary>Which power a key fires: key 1 to 4 is the power the player earned first to fourth (RunProgress's earned list).</summary>
    public static class PowerKeyMap
    {
        /// <summary>Power keys 1 to 4.</summary>
        public const int SlotCount = 4;

        /// <summary>The power on the key slot (0 to 3); false for an empty slot, so the key does nothing.</summary>
        public static bool TryGetPower(IReadOnlyList<StoryPower> earned, int slot, out StoryPower power)
        {
            power = StoryPower.None;
            if (earned == null || slot < 0 || slot >= SlotCount || slot >= earned.Count || earned[slot] == StoryPower.None) return false;
            power = earned[slot];
            return true;
        }
    }
}
