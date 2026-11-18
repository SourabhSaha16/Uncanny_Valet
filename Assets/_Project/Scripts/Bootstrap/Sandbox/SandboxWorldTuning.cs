using UncannyValet.Core.Contracts;
using UncannyValet.Run;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>Applies what a lot decides about driving to the live tuning store: the mud numbers (the defaults, then the lot's own) and the rest of the lot's seeding, as the real lot does at its start.</summary>
    public static class SandboxWorldTuning
    {
        /// <summary>Writes the lot's numbers into the store, publishing each change. The mud defaults go first, so a world without mud also loses the last world's mud numbers.</summary>
        public static void Apply(ITuning store, LotDefinition lot, float gameMinutesPerSecond)
        {
            var live = new SetThroughTuning(store);
            MudRows.SeedDefaults(live);
            lot.SeedPacing(live, gameMinutesPerSecond);
        }
    }
}
