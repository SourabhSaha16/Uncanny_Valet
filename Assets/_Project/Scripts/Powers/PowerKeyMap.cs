using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Powers
{
    /// <summary>Which power a key fires: key 1 to 4 is the power earned first to fourth. Only powers with an effect fire.</summary>
    public static class PowerKeyMap
    {
        /// <summary>Power keys 1 to 4.</summary>
        public const int SlotCount = 4;

        /// <summary>The power on the key slot (0 to 3), when one is owned there and has an effect.</summary>
        public static bool TryGetKind(IReadOnlyList<StoryPower> earned, int slot, out PowerKind kind)
        {
            kind = default;
            if (earned == null || slot < 0 || slot >= earned.Count) return false;
            if (earned[slot] != StoryPower.FreezeTime) return false;
            kind = PowerKind.FreezeTime;
            return true;
        }
    }
}
