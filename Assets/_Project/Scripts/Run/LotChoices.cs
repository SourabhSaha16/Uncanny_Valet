using System.Collections.Generic;
using UncannyValet.Core.Contracts;

namespace UncannyValet.Run
{
    /// <summary>Builds the title-screen lot picker list from the run definition (one entry per lot, in play order).</summary>
    public static class LotChoices
    {
        public static List<LotChoice> From(RunDefinition run)
        {
            var list = new List<LotChoice>();
            if (run == null) return list;
            for (var i = 0; i < run.Lots.Count; i++)
                if (run.Lots[i] != null) list.Add(new LotChoice(run.Lots[i].Id, run.Lots[i].DisplayName));
            return list;
        }
    }
}
