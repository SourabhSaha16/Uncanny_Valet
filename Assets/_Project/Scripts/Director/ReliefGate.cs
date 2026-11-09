using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>
    /// Pure relief state machine (spec section 3): when the player is drowning, hold all arrivals for a while, then
    /// allow only plain cars for the next few, then a cooldown before relief can start again.
    /// </summary>
    public sealed class ReliefGate
    {
        private readonly ReliefSettings _settings;
        private float _holdLeft;
        private float _cooldownLeft;
        private int _plainLeft;

        public ReliefGate(ReliefSettings settings) { _settings = settings; }

        public ReliefStage Stage { get; private set; }

        public bool Active => Stage != ReliefStage.None;

        public void Reset()
        {
            Stage = ReliefStage.None;
            _holdLeft = 0f;
            _cooldownLeft = 0f;
            _plainLeft = 0;
        }

        /// <summary>Counts the hold and the cooldown down. Returns true when relief ended with this tick.</summary>
        public bool Tick(float dt)
        {
            if (_cooldownLeft > 0f) _cooldownLeft -= dt;
            if (Stage != ReliefStage.Holding) return false;
            _holdLeft -= dt;
            if (_holdLeft > 0f) return false;
            if (_plainLeft > 0) { Stage = ReliefStage.Plain; return false; }
            End();
            return true;
        }

        /// <summary>Starts relief when the player is drowning and relief is neither running nor cooling down.</summary>
        public bool TryStart(float workload, float pressure, int angryRecent, out ReliefTrigger trigger)
        {
            trigger = ReliefTrigger.Workload;
            if (Active || _cooldownLeft > 0f) return false;
            if (workload > _settings.Workload) trigger = ReliefTrigger.Workload;
            else if (pressure > _settings.Pressure) trigger = ReliefTrigger.Pressure;
            else if (angryRecent >= _settings.AngryCount) trigger = ReliefTrigger.Angry;
            else return false;
            Stage = ReliefStage.Holding;
            _holdLeft = _settings.HoldSeconds;
            _plainLeft = _settings.PlainCars;
            return true;
        }

        /// <summary>A car was sent. Returns true when that was the last relief plain car.</summary>
        public bool CarSent()
        {
            if (Stage != ReliefStage.Plain) return false;
            _plainLeft--;
            if (_plainLeft > 0) return false;
            End();
            return true;
        }

        private void End()
        {
            Stage = ReliefStage.None;
            _cooldownLeft = _settings.CooldownSeconds;
        }
    }
}
