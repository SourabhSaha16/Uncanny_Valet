using UncannyValet.Core.Events;

namespace UncannyValet.Powers
{
    /// <summary>One valet power. New powers are new classes behind this interface.</summary>
    public interface IPowerEffect
    {
        PowerKind Kind { get; }

        /// <summary>True while a timed power runs. Instant powers are never active.</summary>
        bool IsActive { get; }

        /// <summary>Starts the power; firing it again while active refreshes the timer (never stronger).</summary>
        void Activate();

        /// <summary>Advances the power's own timer by real seconds.</summary>
        void Tick(float deltaSeconds);

        /// <summary>Stops the power at once.</summary>
        void End();
    }
}
