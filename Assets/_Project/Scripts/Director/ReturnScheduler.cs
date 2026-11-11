using System.Collections.Generic;
using UncannyValet.Core.Contracts;

namespace UncannyValet.Director
{
    /// <summary>
    /// Pure return scheduling (spec section 5, issue 73). It only ever looks at cars whose patron is still
    /// inside the diner (dropped off, stay not over, not yet summoned), wherever the car is, so it can never
    /// move the return of a patron who already left or asked for the car.
    /// Before the late-shift phase it keeps every return inside its window (the planned stay +/- the window share),
    /// keeps patrons at least the pickup spacing apart (waived in a wave peak), brings one return forward when the lot
    /// is tight or the player is under target, and holds one back when over target or in relief, at most once per
    /// cooldown. In the late-shift phase <see cref="LateReturnPlanner"/> pulls every return forward outside the window.
    /// </summary>
    public sealed class ReturnScheduler
    {
        private readonly ReturnSettings _settings;
        private readonly ReturnCandidates _cars = new ReturnCandidates();
        private readonly LateReturnPlanner _late;
        private float _lastNudge = float.NegativeInfinity;

        public ReturnScheduler(ReturnSettings settings)
        {
            _settings = settings;
            _late = new LateReturnPlanner(settings);
        }

        /// <summary>True from the late-shift start until the end of the shift.</summary>
        public bool IsLate(in ReturnInput input)
            => _settings.LateStartSeconds > 0f && input.SecondsLeft <= _settings.LateStartSeconds;

        /// <summary>Clears the cooldown and the late plan for a new shift.</summary>
        public void Reset()
        {
            _lastNudge = float.NegativeInfinity;
            _late.Reset();
        }

        /// <summary>Appends the returns to move to <paramref name="output"/> (not cleared); returns how many were added.</summary>
        public int Plan(in LotSnapshot lot, in ReturnInput input, List<ReturnShift> output)
        {
            _cars.Collect(lot, _settings);
            if (_cars.Count == 0) return 0;
            if (IsLate(input)) _late.Plan(_cars, input);
            else PlanNormal(input);
            return _cars.Emit(output, _settings.MinMoveSeconds);
        }

        private void PlanNormal(in ReturnInput input)
        {
            if (!input.WavePeak) KeepSpacing(input.LastSummonedSeconds);
            if (input.NowSeconds - _lastNudge < _settings.CooldownSeconds) return;
            var forward = !input.InRelief && (input.LotTight || input.Gap < -input.Band);
            var hold = input.InRelief || input.Gap > input.Band;
            var moved = forward ? BringForward(input.LastSummonedSeconds) : hold && HoldBack();
            if (moved) _lastNudge = input.NowSeconds;
        }

        // Pushes a return later (inside its window) when it is closer than the spacing to the one before it; when it
        // cannot move later, pulls the one before it earlier instead (inside its own window).
        private void KeepSpacing(float lastSummoned)
        {
            var prev = lastSummoned;
            for (var i = 0; i < _cars.Count; i++)
            {
                var c = _cars[i];
                var wanted = prev + _settings.SpacingSeconds;
                if (c.At < wanted)
                {
                    if (wanted <= c.Hi) _cars.Move(i, wanted);
                    else if (i > 0) PullEarlier(i - 1, c.At - _settings.SpacingSeconds, i > 1 ? _cars[i - 2].At : lastSummoned);
                }
                prev = System.Math.Max(prev, _cars[i].At);
            }
        }

        // Moves a car earlier toward the target time, but not before its window or the spacing after the one before it.
        private void PullEarlier(int index, float to, float beforeAt)
        {
            var c = _cars[index];
            var earliest = System.Math.Max(c.Lo, beforeAt + _settings.SpacingSeconds);
            if (System.Math.Max(to, earliest) < c.At) _cars.Move(index, System.Math.Max(to, earliest));
        }

        // Brings the earliest return that has room as far forward as the window and the spacing allow.
        private bool BringForward(float lastSummoned)
        {
            var prev = lastSummoned;
            for (var i = 0; i < _cars.Count; i++)
            {
                var c = _cars[i];
                var to = System.Math.Max(c.Lo, prev + _settings.SpacingSeconds);
                if (c.At - to >= _settings.MinMoveSeconds) { _cars.Move(i, to); return true; }
                prev = System.Math.Max(prev, c.At);
            }
            return false;
        }

        // Holds back the earliest return that has room, never closer than the spacing to the next one.
        private bool HoldBack()
        {
            for (var i = 0; i < _cars.Count; i++)
            {
                var c = _cars[i];
                var next = i + 1 < _cars.Count ? _cars[i + 1].At - _settings.SpacingSeconds : float.PositiveInfinity;
                var to = System.Math.Min(c.Hi, next);
                if (to - c.At >= _settings.MinMoveSeconds) { _cars.Move(i, to); return true; }
            }
            return false;
        }
    }
}
