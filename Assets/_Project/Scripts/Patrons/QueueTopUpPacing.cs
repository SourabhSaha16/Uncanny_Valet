using System;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Pure rule for crazy lots: the pickup queue is always full. Whenever a slot is free a car is due, at most one
    /// per spacing so arriving cars do not appear on top of each other.
    /// </summary>
    public sealed class QueueTopUpPacing : IArrivalPacing
    {
        private readonly float _spacingMinutes;
        private float _nextMinute;

        /// <param name="spacingMinutes">Game minutes between two sends; 0 sends every free slot at once.</param>
        public QueueTopUpPacing(float spacingMinutes) { _spacingMinutes = Math.Max(0f, spacingMinutes); }

        public bool TopsUpQueue => true;

        public bool Due(float elapsedMinutes) => elapsedMinutes >= _nextMinute;

        public void Spawned(float elapsedMinutes) => _nextMinute = elapsedMinutes + _spacingMinutes;

        public void Reset() => _nextMinute = 0f;
    }
}
