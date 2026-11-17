using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;

namespace UncannyValet.Powers
{
    /// <summary>One valet power. New powers are new classes behind this interface; the framework owns keys, bucket, cooldown and level.</summary>
    public interface IPower
    {
        StoryPower Power { get; }

        /// <summary>True while a power with a duration runs. Instant powers are never active.</summary>
        bool IsActive { get; }

        /// <summary>Share of the running power's time left, 1 down to 0; 0 when it is not active.</summary>
        float Remaining01 { get; }

        /// <summary>
        /// True when the power can start now (for example a target exists). When false no bucket is spent and no
        /// cooldown starts. Called before the bucket is spent, so it must not change anything.
        /// </summary>
        bool CanActivate();

        /// <summary>Starts the power with the numbers of its current level. Firing again while active restarts it (never stronger).</summary>
        void Activate(PowerLevelValues values);

        /// <summary>Advances the power's own timer by real seconds.</summary>
        void Tick(float deltaSeconds);

        /// <summary>Stops the power at once.</summary>
        void End();
    }
}
