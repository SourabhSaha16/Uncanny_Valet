using System.Collections.Generic;
using UncannyValet.Core.Contracts;

namespace UncannyValet.Director
{
    /// <summary>
    /// The open tickets whose patron is still inside the diner (stay not over), sorted by return time, as a reusable working list for
    /// one scheduler tick. The car may be anywhere; only a ticket that already asked for the car, whose patron is on
    /// the way to the curb, or whose stay has not started (the car is still in the queue: no return time known, so
    /// nothing to move, in the late phase too) is left out.
    /// </summary>
    internal sealed class ReturnCandidates
    {
        private readonly List<ReturnCandidate> _items = new List<ReturnCandidate>();

        public int Count => _items.Count;

        public ReturnCandidate this[int index] => _items[index];

        /// <summary>Sets the working return time of one candidate.</summary>
        public void Move(int index, float at)
        {
            var c = _items[index];
            c.At = at;
            _items[index] = c;
        }

        /// <summary>Refills the list from the snapshot, earliest return first.</summary>
        public void Collect(in LotSnapshot lot, ReturnSettings settings)
        {
            _items.Clear();
            for (var i = 0; i < lot.Cars.Count; i++)
            {
                var car = lot.Cars[i];
                if (car.State == CarLotState.PickupRequested) continue;
                if (!(car.SecondsToPickup > 0f)) continue;
                if (float.IsInfinity(car.SecondsToPickup)) continue;
                Insert(Make(car, lot.NowSeconds, settings));
            }
        }

        /// <summary>Writes every candidate that really moved into <paramref name="output"/>; returns how many.</summary>
        public int Emit(List<ReturnShift> output, float minMoveSeconds)
        {
            var n = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                var c = _items[i];
                var delta = c.At - c.Start;
                if (delta < minMoveSeconds && delta > -minMoveSeconds) continue;
                output.Add(new ReturnShift(c.Car, delta));
                n++;
            }
            return n;
        }

        // The window is the planned stay +/- the window share, around the unmoved return (this one minus the shift).
        private static ReturnCandidate Make(in CarSnapshot car, float now, ReturnSettings settings)
        {
            var at = now + car.SecondsToPickup;
            var original = at - car.ReturnShiftSeconds;
            var room = car.PlannedStaySeconds * settings.WindowFraction;
            var earliest = now + settings.MinLeadSeconds;
            return new ReturnCandidate
            {
                Car = car.Car, At = at, Start = at,
                Lo = System.Math.Max(original - room, earliest),
                Hi = System.Math.Max(original + room, at),
            };
        }

        private void Insert(in ReturnCandidate c)
        {
            var i = _items.Count;
            _items.Add(c);
            while (i > 0 && _items[i - 1].Start > c.Start)
            {
                _items[i] = _items[i - 1];
                i--;
            }
            _items[i] = c;
        }
    }
}
