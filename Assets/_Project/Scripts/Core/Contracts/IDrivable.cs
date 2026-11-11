using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>A car the valet can drive. Implemented by the Cars module.</summary>
    public interface IDrivable
    {
        CarId Id { get; }
        Transform Transform { get; }

        /// <summary>Current speed in world units per second.</summary>
        float Speed { get; }

        /// <summary>True while the valet is in the driver's seat.</summary>
        bool HasDriver { get; }

        /// <summary>Throttle and steer in -1..1. Called every frame while driven.</summary>
        void SetInput(float throttle, float steer);

        /// <summary>True while the handbrake (Drift) button is held. Called every frame while driven.</summary>
        void SetDrift(bool drift);

        void SetDriver(bool hasDriver);
    }
}
