using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Where the player is: the valet on foot, or the car he drives (the valet follows it). Registered by Bootstrap.</summary>
    public interface IPlayerFocus
    {
        /// <summary>World position of the valet or of the car he is driving.</summary>
        Vector2 Position { get; }
    }
}
