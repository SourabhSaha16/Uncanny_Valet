using System;

namespace UncannyValet.Audio
{
    /// <summary>Maps each <see cref="SfxId"/> to the code that synthesises it.</summary>
    public static class SfxRecipeBook
    {
        /// <summary>Renders the effect to mono samples in [-1, 1].</summary>
        public static float[] Render(SfxId id, int sampleRate) => Render(id, sampleRate, SfxTyres.DefaultSharpness);

        /// <summary>Renders the effect; <paramref name="tyreSharpness"/> (0..1) only changes the tyre sounds.</summary>
        public static float[] Render(SfxId id, int sampleRate, float tyreSharpness)
        {
            switch (id)
            {
                case SfxId.CarEnter: return SfxActions.CarEnter(sampleRate);
                case SfxId.CarExit: return SfxActions.CarExit(sampleRate);
                case SfxId.CarParked: return SfxActions.CarParked(sampleRate);
                case SfxId.BlessingEarned: return SfxActions.BlessingEarned(sampleRate);
                case SfxId.PickupRequested: return SfxAlerts.PickupRequested(sampleRate);
                case SfxId.CarVanished: return SfxAlerts.CarVanished(sampleRate);
                case SfxId.CarArrives: return SfxAlerts.CarArrives(sampleRate);
                case SfxId.NightEnded: return SfxAlerts.NightEnded(sampleRate);
                case SfxId.ButtonClick: return SfxAlerts.ButtonClick(sampleRate);
                case SfxId.Charged: return SfxStations.Charged(sampleRate);
                case SfxId.Washed: return SfxStations.Washed(sampleRate);
                case SfxId.StationStart: return SfxStations.StationStart(sampleRate);
                case SfxId.PedestrianBump: return SfxAlerts.PedestrianBump(sampleRate);
                case SfxId.CarBump: return SfxAlerts.CarBump(sampleRate);
                case SfxId.PerfectDing: return SfxAlerts.PerfectDing(sampleRate);
                case SfxId.TimeStop: return SfxFreeze.TimeStop(sampleRate);
                case SfxId.TimeResume: return SfxFreeze.TimeResume(sampleRate);
                case SfxId.ThunderCrack: return SfxThunder.Crack(sampleRate);
                case SfxId.PedestrianYelp: return SfxYelp.Yelp(sampleRate);
                case SfxId.TyreChirp: return SfxTyres.Chirp(sampleRate, tyreSharpness);
                case SfxId.TyreScreech: return SfxTyres.Screech(sampleRate, tyreSharpness);
                default: throw new ArgumentOutOfRangeException(nameof(id), id, "No recipe for this effect.");
            }
        }
    }
}
