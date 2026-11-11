using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Cars
{
    /// <summary>What every car of one factory shares: the handling config, its live values and the bump rate limiter.</summary>
    public sealed class CarHandlingKit : IDisposable
    {
        private CarHandlingTuningLink _link;

        public CarHandlingKit(CarHandlingConfig config)
        {
            Config = config;
            Live = new CarHandlingLive(config);
            Limiter = new CarBumpLimiter(config.BumpPairInterval);
        }

        /// <summary>The read-only config asset.</summary>
        public CarHandlingConfig Config { get; }
        /// <summary>The live numbers the F2 panel edits.</summary>
        public CarHandlingLive Live { get; }
        /// <summary>Per-pair limiter of <see cref="CarsCollided"/> events.</summary>
        public CarBumpLimiter Limiter { get; }

        /// <summary>Starts applying panel changes (and the values already in the store) to <see cref="Live"/>.</summary>
        public void Link(IEventBus bus, ITuning tuning) => _link = new CarHandlingTuningLink(bus, tuning, Live);

        public void Dispose() => _link?.Dispose();
    }
}
