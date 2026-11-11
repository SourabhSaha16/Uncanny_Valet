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

        /// <summary>True while the shared Dash/Handbrake button is held (Left Shift, gamepad right shoulder). Driving: handbrake drift; on foot: dash.</summary>
        bool Drift { get; }
    }
}
