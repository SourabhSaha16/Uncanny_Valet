using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Powers
{
    /// <summary>
    /// Owns the running powers and their cooldowns. A power fires only when it is owned, ready (not running, not cooling
    /// down), a bucket is open and it says it can start; the bucket is spent only then. The cooldown starts when the use
    /// ends (at once for an instant power). Each power runs on its own timer, so one keeps going while another fires.
    /// </summary>
    public sealed class PowerStack
    {
        private readonly IEventBus _bus;
        private readonly IPower[] _powers;
        private readonly float[] _cooldownAfterUse;

        public PowerStack(IEventBus bus, IPower[] powers, PowerCooldowns cooldowns)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _powers = powers ?? throw new ArgumentNullException(nameof(powers));
            Cooldowns = cooldowns ?? throw new ArgumentNullException(nameof(cooldowns));
            _cooldownAfterUse = new float[powers.Length];
        }

        /// <summary>The cooldown clocks.</summary>
        public PowerCooldowns Cooldowns { get; }

        public bool IsActive(StoryPower power)
        {
            var index = IndexOf(power);
            return index >= 0 && _powers[index].IsActive;
        }

        /// <summary>The use-cycle phase of a power and the share of the running or cooling time left (0 when ready).</summary>
        public PowerPhase PhaseOf(StoryPower power, out float remaining01)
        {
            remaining01 = 0f;
            var index = IndexOf(power);
            if (index >= 0 && _powers[index].IsActive)
            {
                remaining01 = _powers[index].Remaining01;
                return PowerPhase.Active;
            }
            if (Cooldowns.IsReady(power)) return PowerPhase.Ready;
            remaining01 = Cooldowns.Remaining01(power);
            return PowerPhase.Cooling;
        }

        /// <summary>
        /// Starts the power at <paramref name="level"/> (0 means not owned) when every rule allows it, spends one bucket and
        /// publishes PowerActivated.
        /// </summary>
        public FireResult TryFire(StoryPower power, int level, PowerLevelValues values, IPowerBucket bucket)
        {
            if (level < 1) return FireResult.NotOwned;
            var index = IndexOf(power);
            if (index < 0) return FireResult.NoPower;
            if (_powers[index].IsActive || !Cooldowns.IsReady(power)) return FireResult.NotReady;
            if (!bucket.CanSpend) return FireResult.NoBucket;
            if (!_powers[index].CanActivate()) return FireResult.CannotActivate;
            if (!bucket.TrySpend()) return FireResult.NoBucket;
            _powers[index].Activate(values);
            _cooldownAfterUse[index] = values.Cooldown;
            _bus.Publish(new PowerActivated(power, level));
            if (!_powers[index].IsActive) Cooldowns.Start(power, values.Cooldown);
            return FireResult.Started;
        }

        /// <summary>Advances every power and cooldown by real seconds; a power that ran out publishes PowerEnded and starts its cooldown.</summary>
        public void Tick(float deltaSeconds)
        {
            Cooldowns.Tick(deltaSeconds);
            for (var i = 0; i < _powers.Length; i++)
            {
                var wasActive = _powers[i].IsActive;
                _powers[i].Tick(deltaSeconds);
                if (!wasActive || _powers[i].IsActive) continue;
                Cooldowns.Start(_powers[i].Power, _cooldownAfterUse[i]);
                _bus.Publish(new PowerEnded(_powers[i].Power));
            }
        }

        /// <summary>A new lot starts clean: stops every running power (publishing PowerEnded) and makes every cooldown ready.</summary>
        public void EndAll()
        {
            for (var i = 0; i < _powers.Length; i++)
            {
                if (!_powers[i].IsActive) continue;
                _powers[i].End();
                _bus.Publish(new PowerEnded(_powers[i].Power));
            }
            Cooldowns.ResetAll();
        }

        private int IndexOf(StoryPower power)
        {
            for (var i = 0; i < _powers.Length; i++)
                if (_powers[i].Power == power) return i;
            return -1;
        }
    }
}
