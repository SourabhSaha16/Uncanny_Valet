using System.Collections.Generic;
using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// What the debug panel shows: every key, grouped, with a sensible range. The difficulty dials and the clock
    /// come first and are always visible; every other row is advanced. Rows of one group stay together.
    /// </summary>
    public static class TuningCatalog
    {
        public static readonly IReadOnlyList<TuningInfo> All = Build();

        private static List<TuningInfo> Build()
        {
            var all = new List<TuningInfo>();
            all.AddRange(DifficultyRows.Rows);
            all.AddRange(GameRows.Clock);
            all.AddRange(ArrivalRows.Rows);
            all.AddRange(ReturnRows.Rows);
            all.AddRange(QueueRows.Rows);
            all.AddRange(DirectorPacingRows.PacingRows);
            all.AddRange(DirectorMixRows.MixRows);
            all.AddRange(DirectorMixRows.DilemmaRows);
            all.AddRange(DirectorPlayerRows.MercyRows);
            all.AddRange(DirectorPlayerRows.SkillRows);
            all.AddRange(DirectorPacingRows.CostRows);
            all.AddRange(GameRows.Rest);
            all.AddRange(HandlingRows.Rows);
            all.AddRange(HandlingRows.TyreRows);
            all.AddRange(PowerRows.Rows);
            all.AddRange(PowerRows.DeveloperRows);
            all.AddRange(PerfectRows.Rows);
            all.AddRange(EmotionRows.Rows);
            all.AddRange(DamageRows.Rows);
            all.AddRange(ShakeRows.Rows);
            return all;
        }
    }
}
