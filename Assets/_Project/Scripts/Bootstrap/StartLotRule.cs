using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.Run;

namespace UncannyValet.Bootstrap
{
    /// <summary>Where the story starts: the first lot of the run that is not cleared yet (a fresh run: its first lot, Midgard's training lot).</summary>
    public static class StartLotRule
    {
        /// <summary>The first uncleared lot in play order; the first lot when all are cleared; the default id for an empty run.</summary>
        public static LotId First(RunDefinition run, IRunProgress progress)
        {
            var lots = run.Lots;
            for (var i = 0; i < lots.Count; i++)
                if (lots[i] != null && !progress.IsCleared(lots[i].Id)) return lots[i].Id;
            return lots.Count > 0 && lots[0] != null ? lots[0].Id : default;
        }
    }
}
