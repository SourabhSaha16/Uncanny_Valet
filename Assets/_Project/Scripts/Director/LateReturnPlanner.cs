using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Director
{
    /// <summary>
    /// The end-of-shift phase (issue 73): every parked car's patron is called back early, whatever the return window,
    /// so the player can still collect blessings (blessings are paid only for pickups). The first batch is spread evenly from now
    /// to the last useful moment; a car that parks later takes the first free slot. No two returns are closer than the
    /// pickup spacing. A car is planned once; later ticks only keep the spacing, so the spread does not collapse into a
    /// rush as time passes. A car counts wherever it is (parked, driven, washing or charging) once its stay has started; a car still in the queue with no stay started has no return time and is left alone (the queue wait limit starts it).
    /// </summary>
    internal sealed class LateReturnPlanner
    {
        private readonly ReturnSettings _settings;
        private readonly List<int> _planned = new List<int>();

        public LateReturnPlanner(ReturnSettings settings) { _settings = settings; }

        public void Reset() => _planned.Clear();

        /// <summary>Plans the late phase for the candidates (ascending by return time), moving them in place.</summary>
        public void Plan(ReturnCandidates cars, in ReturnInput input)
        {
            KeepFixedSpacing(cars, input.LastSummonedSeconds);
            var earliest = System.Math.Max(input.NowSeconds + _settings.MinLeadSeconds,
                input.LastSummonedSeconds + _settings.SpacingSeconds);
            var deadline = input.NowSeconds + input.SecondsLeft - _settings.LastLeadSeconds;
            var firstBatch = _planned.Count == 0;
            var remaining = CountUnplanned(cars);
            var step = System.Math.Max(_settings.SpacingSeconds, (deadline - earliest) / System.Math.Max(remaining, 1));
            var k = 0;
            for (var i = 0; i < cars.Count; i++)
            {
                var c = cars[i];
                if (IsPlanned(c.Car)) continue;
                var wanted = System.Math.Min(c.At, firstBatch ? earliest + k * step : earliest);
                var floor = System.Math.Min(c.At, earliest);
                var at = FirstFree(cars, i, System.Math.Max(wanted, floor), input.LastSummonedSeconds);
                cars.Move(i, at > deadline ? c.At : at);
                _planned.Add(c.Car.Value);
                k++;
            }
        }

        // Cars planned earlier stay where they are and are only pushed later to keep the spacing.
        private void KeepFixedSpacing(ReturnCandidates cars, float lastSummoned)
        {
            var prev = lastSummoned;
            for (var i = 0; i < cars.Count; i++)
            {
                var c = cars[i];
                if (IsPlanned(c.Car))
                {
                    var at = System.Math.Max(c.At, prev + _settings.SpacingSeconds);
                    cars.Move(i, at);
                    prev = System.Math.Max(prev, at);
                }
            }
        }

        // The first time at or after t that is at least the spacing away from every car already placed.
        private float FirstFree(ReturnCandidates cars, int self, float t, float lastSummoned)
        {
            var spacing = _settings.SpacingSeconds;
            for (var pass = 0; pass <= cars.Count; pass++)
            {
                var moved = false;
                for (var j = 0; j < cars.Count; j++)
                {
                    if (j == self || !IsPlanned(cars[j].Car)) continue;
                    var gap = t - cars[j].At;
                    if (gap >= spacing || gap <= -spacing) continue;
                    t = cars[j].At + spacing;
                    moved = true;
                }
                if (!moved) break;
            }
            return System.Math.Max(t, lastSummoned + spacing);
        }

        private int CountUnplanned(ReturnCandidates cars)
        {
            var n = 0;
            for (var i = 0; i < cars.Count; i++) if (!IsPlanned(cars[i].Car)) n++;
            return n;
        }

        private bool IsPlanned(CarId car) => _planned.Contains(car.Value);
    }
}
