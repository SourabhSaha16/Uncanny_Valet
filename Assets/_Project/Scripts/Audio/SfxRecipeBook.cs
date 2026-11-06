using System;

namespace UncannyValet.Audio
{
    /// <summary>Maps each <see cref="SfxId"/> to the code that synthesises it.</summary>
    public static class SfxRecipeBook
    {
        /// <summary>Renders the effect to mono samples in [-1, 1].</summary>
        public static float[] Render(SfxId id, int sampleRate)
        {
            switch (id)
            {
                case SfxId.CarEnter: return SfxActions.CarEnter(sampleRate);
                case SfxId.CarExit: return SfxActions.CarExit(sampleRate);
                case SfxId.CarParked: return SfxActions.CarParked(sampleRate);
                case SfxId.TipEarned: return SfxActions.TipEarned(sampleRate);
                case SfxId.PickupRequested: return SfxAlerts.PickupRequested(sampleRate);
                case SfxId.CarVanished: return SfxAlerts.CarVanished(sampleRate);
                case SfxId.CarArrives: return SfxAlerts.CarArrives(sampleRate);
                case SfxId.NightEnded: return SfxAlerts.NightEnded(sampleRate);
                case SfxId.ButtonClick: return SfxAlerts.ButtonClick(sampleRate);
                default: throw new ArgumentOutOfRangeException(nameof(id), id, "No recipe for this effect.");
            }
        }
    }
}
