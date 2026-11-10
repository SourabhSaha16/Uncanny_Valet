using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>
    /// Pure arrival pacing (spec section 3): decides each tick whether to wait, hold or send a car. The countdown is a
    /// progress through the current interval, and the interval is scaled by a factor that slews toward its goal, so a
    /// change in the player's workload re-scales the countdown smoothly instead of resetting it. The arrival budget
    /// pulls the interval toward an even spread of the remaining cars over the area under the target curve.
    /// </summary>
    public sealed class ArrivalPacer
    {
        private const float Epsilon = 0.001f;

        private readonly IDirectorRandom _random;
        private readonly PacingSettings _settings;
        private readonly ReliefGate _relief;
        private float _progress;
        private float _baseInterval;
        private float _factor = 1f;
        private float _idleTimer;
        private bool _awaitingSpawn;
        private float _awaitingSeconds;

        public ArrivalPacer(IDirectorRandom random, PacingSettings settings, ReliefGate relief)
        {
            _random = random; _settings = settings; _relief = relief;
        }

        /// <summary>True while relief holds arrivals or restricts them to plain cars.</summary>
        public bool InRelief => _relief.Active;

        /// <summary>Clears the countdown and relief for a new shift.</summary>
        public void Reset()
        {
            _progress = 0f;
            _baseInterval = 0f;
            _factor = 1f;
            _idleTimer = 0f;
            _awaitingSpawn = false;
            _awaitingSeconds = 0f;
            _relief.Reset();
        }

        /// <summary>A car really arrived (the spawner executed the schedule). Returns true when relief ended with it.</summary>
        public bool NotifySpawned()
        {
            _awaitingSpawn = false;
            _progress = 0f;
            _baseInterval = 0f;
            _idleTimer = 0f;
            return _relief.CarSent();
        }

        public PacerDecision Tick(in PacerInput input, float dt)
        {
            var ended = _relief.Tick(dt);
            var started = _relief.TryStart(input.Workload, input.Pressure, input.AngryRecent, out var trigger);
            if (_relief.Stage == ReliefStage.Holding) return Make(PacerAction.Hold, HoldReason.Relief, 0f, started, trigger, ended);
            if (input.ArrivalsUsed >= _settings.MaxArrivals) return Make(PacerAction.Hold, HoldReason.BudgetSpent, 0f, started, trigger, ended);
            if (_awaitingSpawn && StillAwaiting(dt)) return Make(PacerAction.Wait, HoldReason.None, 0f, started, trigger, ended);

            UpdateIdle(input, dt);
            var idle = _idleTimer >= _settings.IdleSeconds && BehindSchedule(input);
            var band = Classify(input, idle);
            _factor = Mathf.MoveTowards(_factor, FactorFor(band), _settings.FactorSlewPerSecond * dt);
            var interval = CurrentInterval(input);
            _progress += dt / interval;
            if (_progress < 1f && !idle) return Make(PacerAction.Wait, HoldReason.None, SecondsLeft(interval), started, trigger, ended);
            if (input.Queue >= input.QueueCap) return Make(PacerAction.Hold, HoldReason.QueueFull, 0f, started, trigger, ended);

            _awaitingSpawn = true;
            _awaitingSeconds = 0f;
            var plain = _relief.Stage == ReliefStage.Plain;
            var reason = plain ? ScheduleReason.ReliefCar : band;
            return new PacerDecision(PacerAction.Spawn, reason, HoldReason.None, plain, 0f, started, trigger, ended);
        }

        // A send the spawner never executed (no clear slot, request lost) must not wedge the pacer: after the
        // acknowledgement timeout the send counts as lost and the due car is sent again.
        private bool StillAwaiting(float dt)
        {
            _awaitingSeconds += dt;
            if (_awaitingSeconds < _settings.SpawnAckSeconds) return true;
            _awaitingSpawn = false;
            return false;
        }

        private void UpdateIdle(in PacerInput input, float dt)
            => _idleTimer = input.Workload < _settings.IdleWorkload ? _idleTimer + dt : 0f;

        // No dead air, but never at the cost of the arrival budget: an idle spawn is only allowed while the cars sent
        // so far are not ahead of an even spread of the budget over the area under T.
        private bool BehindSchedule(in PacerInput input)
        {
            if (input.TargetAreaTotal <= 0f) return true;
            var expected = _settings.ArrivalBudget * (1f - input.TargetAreaLeft / input.TargetAreaTotal);
            return input.ArrivalsUsed <= expected;
        }

        private ScheduleReason Classify(in PacerInput input, bool idle)
        {
            if (idle) return ScheduleReason.Idle;
            var gap = input.Gap(_settings.FormInfluence);
            if (gap < -_settings.Band) return ScheduleReason.UnderTarget;
            return gap > _settings.Band ? ScheduleReason.OverTarget : ScheduleReason.OnTarget;
        }

        private float FactorFor(ScheduleReason band)
        {
            switch (band)
            {
                case ScheduleReason.Idle:
                case ScheduleReason.UnderTarget: return _settings.UnderFactor;
                case ScheduleReason.OverTarget: return _settings.OverFactor;
                default: return 1f;
            }
        }

        private float CurrentInterval(in PacerInput input)
        {
            if (_baseInterval <= 0f) _baseInterval = _random.Range(input.BaseIntervalMin, input.BaseIntervalMax);
            var paced = _baseInterval * _factor;
            var blended = Mathf.Lerp(paced, BudgetInterval(input), _settings.BudgetBlend);
            return Mathf.Clamp(blended, _settings.MinIntervalSeconds, _settings.MaxIntervalSeconds);
        }

        // The remaining target cars spread over the remaining area under T: busy stretches get shorter gaps.
        private float BudgetInterval(in PacerInput input)
        {
            var carsLeft = _settings.ArrivalBudget - input.ArrivalsUsed;
            if (carsLeft <= 0 || input.TargetAreaLeft <= 0f) return _settings.MaxIntervalSeconds;
            return input.TargetAreaLeft / (carsLeft * Mathf.Max(input.Target, Epsilon));
        }

        private float SecondsLeft(float interval)
        {
            var left = (1f - _progress) * interval;
            var untilIdle = _settings.IdleSeconds - _idleTimer;
            return Mathf.Max(0f, untilIdle > 0f && untilIdle < left && _idleTimer > 0f ? untilIdle : left);
        }

        private static PacerDecision Make(PacerAction action, HoldReason hold, float seconds, bool started,
            ReliefTrigger trigger, bool ended)
            => new PacerDecision(action, ScheduleReason.OnTarget, hold, false, seconds, started, trigger, ended);
    }
}
