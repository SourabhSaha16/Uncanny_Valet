using System;
using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// Pure rule: keeps the number of ambient cars on screen between a minimum and a target. Below the minimum a car is due
    /// at once; between the minimum and the target one is due after a random gap; at the target none. The lane is random
    /// among the lanes the caller says are open (relevant and with a free entry gap), so directions mix naturally. One seeded generator.
    /// </summary>
    public sealed class AmbientDensity
    {
        private const int OverfillAllowance = 1;

        private readonly System.Random _random;
        private readonly int _lookCount;
        private readonly Vector2 _speedScale;
        private readonly Vector2 _gapSeconds;
        private int _min;
        private int _target;
        private float _timer;

        /// <param name="seed">Random seed.</param>
        /// <param name="lookCount">How many car looks exist (at least 1).</param>
        /// <param name="speedScale">Lowest and highest multiplier on the cruise speed.</param>
        /// <param name="gapSeconds">Shortest and longest wait between cars while the count is between minimum and target.</param>
        /// <param name="minOnScreen">Cars on screen below which a car is due at once.</param>
        /// <param name="targetOnScreen">Cars on screen at which spawning stops.</param>
        public AmbientDensity(int seed, int lookCount, Vector2 speedScale, Vector2 gapSeconds, int minOnScreen, int targetOnScreen)
        {
            _random = new System.Random(seed);
            _lookCount = Math.Max(1, lookCount);
            _speedScale = speedScale;
            _gapSeconds = gapSeconds;
            SetMinOnScreen(minOnScreen);
            SetTargetOnScreen(targetOnScreen);
            _timer = NextGap();
        }

        /// <summary>Cars on screen below which a car is due at once (0 or less switches the minimum off).</summary>
        public void SetMinOnScreen(int count)
        {
            _min = Math.Max(0, count);
            if (_target < _min) _target = _min;
        }

        /// <summary>Cars on screen at which spawning stops; never below the minimum (0 switches ambient traffic off).</summary>
        public void SetTargetOnScreen(int count) => _target = Math.Max(Math.Max(0, count), _min);

        /// <summary>The count spawning aims for.</summary>
        public int Target => _target;

        /// <summary>
        /// Advances the wait. Returns true and fills <paramref name="request"/> when a car is due and a lane in <paramref name="open"/> can take it.
        /// <paramref name="counted"/> is the number of ambient cars on screen or about to be.
        /// </summary>
        public bool Tick(float dt, int counted, LaneSet open, out SpawnRequest request) => Tick(dt, counted, counted, open, out request);

        /// <summary>
        /// As <see cref="Tick(float, int, LaneSet, out SpawnRequest)"/>, where <paramref name="staying"/> (at most <paramref name="counted"/>) leaves out
        /// the cars about to drive off screen. A car is due at once when fewer than the minimum are staying, even if that
        /// takes the count one above the target for a moment; otherwise the target is a hard stop.
        /// </summary>
        public bool Tick(float dt, int counted, int staying, LaneSet open, out SpawnRequest request)
        {
            request = default;
            bool urgent = staying < _min && counted < _target + OverfillAllowance;
            if (!urgent && counted >= _target)
            {
                _timer = NextGap();
                return false;
            }
            if (!urgent)
            {
                _timer -= dt;
                if (_timer > 0f) return false;
            }
            if (!TryRoll(open, out request)) return false;
            _timer = NextGap();
            return true;
        }

        /// <summary>A random car on a random lane of <paramref name="open"/>; false when no lane is open.</summary>
        public bool TryRoll(LaneSet open, out SpawnRequest request)
        {
            request = default;
            int lanes = 0;
            for (int i = 0; i < TrafficLanes.Count; i++)
            {
                if (TrafficLanes.Has(open, (TrafficLane)i)) lanes++;
            }
            if (lanes == 0) return false;
            int pick = _random.Next(lanes);
            for (int i = 0; i < TrafficLanes.Count; i++)
            {
                var lane = (TrafficLane)i;
                if (!TrafficLanes.Has(open, lane) || pick-- > 0) continue;
                float scale = Mathf.Lerp(_speedScale.x, _speedScale.y, NextUnit());
                request = new SpawnRequest(lane, _random.Next(_lookCount), scale);
                return true;
            }
            return false;
        }

        /// <summary>The next random number in 0..1.</summary>
        public float NextUnit() => (float)_random.NextDouble();

        private float NextGap() => Mathf.Lerp(_gapSeconds.x, _gapSeconds.y, NextUnit());
    }
}
