using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// The rule of the free on-foot dash: it starts on a fresh press of the dash button (not while the button is merely held),
    /// only on foot, only when no dash runs and the cooldown has run out. The cooldown starts when a dash finishes.
    /// </summary>
    public sealed class DashCooldown
    {
        private bool _wasHeld;

        /// <summary>Seconds left before the next dash may start; 0 when ready.</summary>
        public float Remaining { get; private set; }

        /// <summary>True from the start of a dash until <see cref="Finish"/>.</summary>
        public bool IsDashing { get; private set; }

        /// <summary>Call every frame with the button state. True when a dash should start now (the rule then counts it as running).</summary>
        public bool TryStart(bool held, bool driving)
        {
            var pressed = held && !_wasHeld;
            _wasHeld = held;
            if (!pressed || driving || IsDashing || Remaining > 0f) return false;
            IsDashing = true;
            return true;
        }

        /// <summary>The dash ended (distance covered, time up, blocked or cancelled): the cooldown starts.</summary>
        public void Finish(float cooldownSeconds)
        {
            IsDashing = false;
            Remaining = Mathf.Max(0f, cooldownSeconds);
        }

        /// <summary>Counts the cooldown down.</summary>
        public void Tick(float deltaSeconds) => Remaining = Mathf.Max(0f, Remaining - deltaSeconds);

        /// <summary>Forgets any dash and cooldown (a new shift).</summary>
        public void Reset()
        {
            IsDashing = false;
            Remaining = 0f;
        }
    }
}
