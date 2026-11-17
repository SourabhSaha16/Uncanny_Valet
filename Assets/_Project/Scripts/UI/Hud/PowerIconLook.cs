using UncannyValet.Core.Contracts;

namespace UncannyValet.UI.Hud
{
    /// <summary>What one power icon looks like: dim or lit, and the ring around it (running or cooling).</summary>
    public readonly struct PowerIconLook
    {
        /// <summary>True when the power cannot be fired now (no open bucket, running or cooling down).</summary>
        public readonly bool Dimmed;
        /// <summary>Share of the ring lit, 0 to 1; 0 draws no ring.</summary>
        public readonly float RingFraction;
        /// <summary>True when the ring counts the running power down; false when it counts the cooldown.</summary>
        public readonly bool RingIsActive;

        private PowerIconLook(bool dimmed, float ringFraction, bool ringIsActive)
        {
            Dimmed = dimmed; RingFraction = ringFraction; RingIsActive = ringIsActive;
        }

        /// <summary>
        /// Lit when ready with an open bucket; dim with no bucket or on cooldown; a running power is lit with its
        /// time ring; a cooling one is dim with a ring of the cooldown left.
        /// </summary>
        public static PowerIconLook Of(PowerState state, int openBuckets)
        {
            switch (state.Phase)
            {
                case PowerPhase.Active: return new PowerIconLook(false, state.Remaining01, true);
                case PowerPhase.Cooling: return new PowerIconLook(true, state.Remaining01, false);
                default: return new PowerIconLook(openBuckets < 1, 0f, false);
            }
        }
    }
}
