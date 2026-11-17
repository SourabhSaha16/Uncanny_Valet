using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Pure rule for lots that must always have cars waiting (issue 178, Vanaheim training): says when the arrival queue
    /// is below the lot's minimum so another car is due, at most one per top-up spacing so cars do not arrive stacked.
    /// A minimum of 0 never asks for a car, so every other lot keeps the Patron Director's pacing alone.
    /// </summary>
    public sealed class MinQueueTopUp
    {
        private readonly ITuning _tuning;
        private readonly QueueTopUpPacing _spacing;

        public MinQueueTopUp(ITuning tuning, float spacingMinutes)
        {
            _tuning = tuning;
            _spacing = new QueueTopUpPacing(spacingMinutes);
        }

        /// <summary>The queue minimum of the current lot (0 = none).</summary>
        public int Minimum => _tuning.TryGet(TuningKey.PlanMinQueuedCars, out var min) ? (int)min : 0;

        /// <summary>True when fewer than the minimum cars are queued and the spacing since the last top-up has passed.</summary>
        public bool Due(int queued, float elapsedMinutes) => queued < Minimum && _spacing.Due(elapsedMinutes);

        /// <summary>Records a top-up car sent at this shift minute.</summary>
        public void Sent(float elapsedMinutes) => _spacing.Spawned(elapsedMinutes);

        /// <summary>Forgets earlier sends; called at every shift start.</summary>
        public void Reset() => _spacing.Reset();
    }
}
