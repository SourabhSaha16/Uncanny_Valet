using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Player input, shared by walking and driving. Implemented by the Valet module.</summary>
    public interface IInputSource
    {
        /// <summary>x = steer/strafe, y = throttle/forward, each in -1..1.</summary>
        Vector2 Move { get; }

        /// <summary>True only on the frame the interact key was pressed.</summary>
        bool InteractPressed { get; }
    }
}
