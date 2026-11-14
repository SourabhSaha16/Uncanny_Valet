using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>The mud puddles of the current lot, in world space. The Lot registers it; cars ask it where mud is. Not registered when the lot has no mud.</summary>
    public interface IMudPatches
    {
        /// <summary>True when the world point is inside any puddle. Allocation free.</summary>
        bool IsInside(Vector2 point);
    }
}
