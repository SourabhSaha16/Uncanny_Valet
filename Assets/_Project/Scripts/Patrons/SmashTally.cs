using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Pure join of the two halves of a Park Smash: the deliveries (Patrons) and the parking (Lot). Either half may arrive
    /// first; once both are in, <see cref="TryComplete"/> hands out the finished <see cref="ParkSmashCompleted"/> and resets.
    /// </summary>
    public sealed class SmashTally
    {
        private bool _hasDeliveries;
        private bool _hasParking;
        private int _delivered;
        private int _tips;
        private int _parked;
        private int _charged;
        private int _washed;

        /// <summary>The patrons' half: tickets closed and the tips paid.</summary>
        public void RecordDeliveries(int delivered, int totalTips)
        {
            _hasDeliveries = true;
            _delivered = delivered;
            _tips = totalTips;
        }

        /// <summary>The Lot's half: cars parked, EVs charged and dirty cars washed.</summary>
        public void RecordParking(int parked, int charged, int washed)
        {
            _hasParking = true;
            _parked = parked;
            _charged = charged;
            _washed = washed;
        }

        /// <summary>True once both halves are in; returns the result and starts over.</summary>
        public bool TryComplete(out ParkSmashCompleted result)
        {
            result = new ParkSmashCompleted(_parked, _charged, _washed, _delivered, _tips);
            if (!_hasDeliveries || !_hasParking) return false;
            Reset();
            return true;
        }

        /// <summary>Forgets a half-finished smash (a new shift).</summary>
        public void Reset()
        {
            _hasDeliveries = false;
            _hasParking = false;
            _delivered = _tips = _parked = _charged = _washed = 0;
        }
    }
}
